// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.V1.Importing.Paperless.Identification;
using Gnomeshade.WebApi.V1.Importing.Paperless.Parsing;

using Microsoft.Extensions.Logging;

using VMelnalksnis.PaperlessDotNet;
using VMelnalksnis.PaperlessDotNet.Documents;

namespace Gnomeshade.WebApi.V1.Importing.Paperless;

/// <inheritdoc />
public sealed class PaperlessService : IPaperlessService
{
	private static readonly StringComparer _comparer = StringComparer.OrdinalIgnoreCase;

	private readonly ILogger<PaperlessService> _logger;
	private readonly IPaperlessClient _paperlessClient;
	private readonly IPaperlessDocumentParser _documentParser;
	private readonly IPurchaseIdentifier _purchaseIdentifier;
	private readonly CurrencyRepository _currencyRepository;
	private readonly UnitRepository _unitRepository;
	private readonly ProductRepository _productRepository;
	private readonly PurchaseRepository _purchaseRepository;
	private readonly DbConnection _dbConnection;

	/// <summary>Initializes a new instance of the <see cref="PaperlessService"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="paperlessClient">Paperless API client.</param>
	/// <param name="documentParser">Paperless document text parser.</param>
	/// <param name="purchaseIdentifier">Purchase identifier from text.</param>
	/// <param name="currencyRepository">Persistence store for <see cref="CurrencyEntity"/>.</param>
	/// <param name="unitRepository">Persistence store for <see cref="UnitEntity"/>.</param>
	/// <param name="productRepository">Persistence store for <see cref="ProductEntity"/>.</param>
	/// <param name="purchaseRepository">Persistence store for <see cref="PurchaseEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	public PaperlessService(
		ILogger<PaperlessService> logger,
		IPaperlessClient paperlessClient,
		IPaperlessDocumentParser documentParser,
		IPurchaseIdentifier purchaseIdentifier,
		CurrencyRepository currencyRepository,
		UnitRepository unitRepository,
		ProductRepository productRepository,
		PurchaseRepository purchaseRepository,
		DbConnection dbConnection)
	{
		_logger = logger;
		_paperlessClient = paperlessClient;
		_documentParser = documentParser;
		_purchaseIdentifier = purchaseIdentifier;
		_currencyRepository = currencyRepository;
		_unitRepository = unitRepository;
		_productRepository = productRepository;
		_purchaseRepository = purchaseRepository;
		_dbConnection = dbConnection;
	}

	/// <inheritdoc />
	public bool IsPaperlessDocumentUri(string uri) => uri.Contains("Paperless", StringComparison.OrdinalIgnoreCase);

	/// <inheritdoc />
	public Task<Document?> GetPaperlessDocument(string uri)
	{
		var uriBuilder = new UriBuilder(uri);
		var pathParts = uriBuilder.Path.Split('/');
		var idString = pathParts.Last();
		var id = int.Parse(idString);
		return _paperlessClient.Documents.Get(id);
	}

	/// <inheritdoc />
	public async Task AddPurchasesToTransaction(Guid ownerId, Guid transactionId, Document document)
	{
		await using var dbTransaction = await _dbConnection.OpenAndBeginTransaction();

		var rawPurchases = _documentParser.ParsePurchases(document);
		var products = (await _productRepository.GetAllAsync(ownerId)).ToList();
		var currencies = await _currencyRepository.GetAllAsync();
		var units = (await _unitRepository.GetAllAsync(ownerId)).ToList();
		var defaultUnit = units.SingleOrDefault(u => u.Name == "Piece");

		var identifiedPurchases = rawPurchases
			.Select(raw => _purchaseIdentifier.IdentifyPurchase(raw, products, currencies, units))
			.Select(async (purchase, index) =>
			{
				// ReSharper disable once AccessToDisposedClosure
				var product = purchase.Score > 50
					? products.Single(p => _comparer.Equals(p.Name, purchase.ClosestProductName))
					: await CreateProduct(ownerId, purchase, units, defaultUnit, dbTransaction);

				var currency = currencies.Single(c => _comparer.Equals(c.AlphabeticCode, purchase.Currency));

				return new PurchaseEntity
				{
					TransactionId = transactionId,
					Price = purchase.Price,
					CurrencyId = currency.Id,
					ProductId = product.Id,
					Amount = purchase.Amount,
					Order = (uint)index,
				};
			});

		var existingPurchases = (await _purchaseRepository.GetAllAsync(transactionId, ownerId, dbTransaction)).ToList();
		foreach (var identifiedTask in identifiedPurchases)
		{
			var identified = await identifiedTask;

			var equivalentPurchases = existingPurchases.Where(existing =>
				existing.Price == identified.Price &&
				existing.CurrencyId == identified.CurrencyId &&
				existing.ProductId == identified.ProductId &&
				existing.Amount == identified.Amount);

			if (equivalentPurchases.Any())
			{
				_logger.LogDebug("Identified purchase {IdentifiedPurchase} already exists", identified);
				continue;
			}

			_logger.LogDebug("Identified purchase {IdentifiedPurchase} does not existing, creating", identified);
			var creatable = identified with
			{
				Id = Guid.NewGuid(),
				OwnerId = ownerId,
				CreatedByUserId = ownerId,
				ModifiedByUserId = ownerId,
			};

			await _purchaseRepository.AddAsync(creatable, dbTransaction);
		}

		await dbTransaction.CommitAsync();
	}

	private async Task<ProductEntity> CreateProduct(
		Guid ownerId,
		IdentifiedPurchase identifiedPurchase,
		IEnumerable<UnitEntity> units,
		UnitEntity? defaultUnit,
		IDbTransaction dbTransaction)
	{
		var unit = identifiedPurchase.Unit is null
			? defaultUnit
			: units
				.Where(u => u.Symbol is not null)
				.SingleOrDefault(u => _comparer.Equals(u?.Symbol, identifiedPurchase.Unit), defaultUnit);

		var product = new ProductEntity
		{
			Id = Guid.NewGuid(),
			OwnerId = ownerId,
			CreatedByUserId = ownerId,
			ModifiedByUserId = ownerId,
			Name = identifiedPurchase.OriginalName,
			UnitId = unit?.Id,
		};

		await _productRepository.AddAsync(product, dbTransaction);
		return await _productRepository.GetByIdAsync(product.Id, ownerId, dbTransaction);
	}
}
