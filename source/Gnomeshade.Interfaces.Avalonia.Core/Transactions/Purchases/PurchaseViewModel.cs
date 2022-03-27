// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Purchases;

/// <summary>Overview of all <see cref="Purchase"/>s of a single <see cref="Transaction"/>.</summary>
public sealed class PurchaseViewModel : OverviewViewModel<PurchaseOverview>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Guid _transactionId;

	private PurchaseViewModel(IGnomeshadeClient gnomeshadeClient, Guid transactionId)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_transactionId = transactionId;
	}

	/// <summary>Initializes a new instance of the <see cref="PurchaseViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="transactionId">The transaction for which to create a purchase overview.</param>
	/// <returns>A new instance of the <see cref="PurchaseViewModel"/> class.</returns>
	public static async Task<PurchaseViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient, Guid transactionId)
	{
		var viewModel = new PurchaseViewModel(gnomeshadeClient, transactionId);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <inheritdoc />
	public override async Task RefreshAsync()
	{
		var purchases = await _gnomeshadeClient.GetPurchasesAsync(_transactionId).ConfigureAwait(false);

		var currenciesTask = _gnomeshadeClient.GetCurrenciesAsync();
		var productIds = purchases.Select(purchase => purchase.ProductId);
		var productsTask = Task.WhenAll(productIds.Select(async id => await _gnomeshadeClient.GetProductAsync(id)));

		await Task.WhenAll(productsTask, currenciesTask).ConfigureAwait(false);
		var currencies = currenciesTask.Result;
		var products = productsTask.Result;

		var overviews = purchases.Select(purchase =>
			new PurchaseOverview(
				purchase.Id,
				purchase.Price,
				currencies.Single(currency => currency.Id == purchase.CurrencyId).AlphabeticCode,
				products.Single(product => product.Id == purchase.ProductId).Name,
				purchase.Amount,
				purchase.DeliveryDate?.ToLocalTime()));

		Rows = new(overviews); // todo sorting
	}
}
