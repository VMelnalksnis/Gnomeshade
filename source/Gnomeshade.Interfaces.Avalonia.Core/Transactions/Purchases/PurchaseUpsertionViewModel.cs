// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Purchases;

/// <summary>Create or update a purchase.</summary>
public sealed class PurchaseUpsertionViewModel : UpsertionViewModel
{
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
	private readonly Guid _transactionId;
	private readonly Guid? _purchaseId;

	private decimal? _price;
	private Currency? _currency;
	private Product? _product;
	private decimal? _amount;
	private DateTimeOffset? _deliveryDate;
	private TimeSpan? _deliveryTime;
	private List<Currency> _currencies;
	private List<Product> _products;

	private PurchaseUpsertionViewModel(
		IGnomeshadeClient gnomeshadeClient,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid transactionId,
		Guid? purchaseId)
		: base(gnomeshadeClient)
	{
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_transactionId = transactionId;
		_purchaseId = purchaseId;
		_currencies = new();
		_products = new();
	}

	/// <summary>Gets a delegate for formatting a currency in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CurrencySelector => AutoCompleteSelectors.Currency;

	/// <summary>Gets a delegate for formatting a product in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> ProductSelector => AutoCompleteSelectors.Product;

	/// <summary>Gets a collection of all currencies.</summary>
	public List<Currency> Currencies
	{
		get => _currencies;
		private set => SetAndNotify(ref _currencies, value);
	}

	/// <summary>Gets a collection of all products.</summary>
	public List<Product> Products
	{
		get => _products;
		private set => SetAndNotify(ref _products, value);
	}

	/// <summary>Gets or sets the amount paid to purchase an <see cref="Amount"/> of <see cref="Product"/>.</summary>
	public decimal? Price
	{
		get => _price;
		set => SetAndNotifyWithGuard(ref _price, value, nameof(Price), CanSaveNames);
	}

	/// <summary>Gets or sets the id of the currency of <see cref="Price"/>.</summary>
	public Currency? Currency
	{
		get => _currency;
		set => SetAndNotifyWithGuard(ref _currency, value, nameof(Currency), CanSaveNames);
	}

	/// <summary>Gets or sets the id of the purchased product.</summary>
	public Product? Product
	{
		get => _product;
		set => SetAndNotifyWithGuard(ref _product, value, nameof(Product), CanSaveNames);
	}

	/// <summary>Gets or sets the amount of <see cref="Product"/> that was purchased.</summary>
	public decimal? Amount
	{
		get => _amount;
		set => SetAndNotifyWithGuard(ref _amount, value, nameof(Amount), CanSaveNames);
	}

	/// <summary>Gets or sets the date when the <see cref="Product"/> was delivered.</summary>
	public DateTimeOffset? DeliveryDate
	{
		get => _deliveryDate;
		set => SetAndNotify(ref _deliveryDate, value);
	}

	/// <summary>Gets or sets the time when the <see cref="Product"/> was delivered.</summary>
	public TimeSpan? DeliveryTime
	{
		get => _deliveryTime;
		set => SetAndNotify(ref _deliveryTime, value);
	}

	/// <inheritdoc />
	public override bool CanSave =>
		Price is not null &&
		Currency is not null &&
		Product is not null &&
		Amount is not null;

	/// <summary>Initializes a new instance of the <see cref="PurchaseUpsertionViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="transactionId">The id of the transaction to which to add the purchase to.</param>
	/// <param name="id">The id of the purchase to edit.</param>
	/// <returns>A new instance of the <see cref="PurchaseUpsertionViewModel"/> class.</returns>
	public static async Task<PurchaseUpsertionViewModel> CreateAsync(
		IGnomeshadeClient gnomeshadeClient,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid transactionId,
		Guid? id = null)
	{
		var viewModel = new PurchaseUpsertionViewModel(gnomeshadeClient, dateTimeZoneProvider, transactionId, id);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <inheritdoc />
	public override async Task RefreshAsync()
	{
		if (_purchaseId is null)
		{
			Currencies = await GnomeshadeClient.GetCurrenciesAsync().ConfigureAwait(false);
			Products = await GnomeshadeClient.GetProductsAsync().ConfigureAwait(false);
		}
		else
		{
			Currencies = await GnomeshadeClient.GetCurrenciesAsync().ConfigureAwait(false);
			Products = await GnomeshadeClient.GetProductsAsync().ConfigureAwait(false);
			var purchase = await GnomeshadeClient.GetPurchaseAsync(_transactionId, _purchaseId.Value)
				.ConfigureAwait(false);
			Price = purchase.Price;
			Currency = Currencies.Single(currency => currency.Id == purchase.CurrencyId);
			Amount = purchase.Amount;
			Product = Products.Single(product => product.Id == purchase.ProductId);
			DeliveryDate = purchase.DeliveryDate?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset();
			DeliveryTime = purchase.DeliveryDate?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset().TimeOfDay;
		}
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var deliveryDate = DeliveryDate.HasValue
			? new LocalDateTime(
					DeliveryDate.Value.Year,
					DeliveryDate.Value.Month,
					DeliveryDate.Value.Day,
					DeliveryTime.GetValueOrDefault().Hours,
					DeliveryTime.GetValueOrDefault().Minutes)
				.InZoneStrictly(_dateTimeZoneProvider.GetSystemDefault())
			: default(ZonedDateTime?);

		var purchaseCreation = new PurchaseCreation
		{
			Price = Price,
			CurrencyId = Currency!.Id,
			Amount = Amount,
			ProductId = Product!.Id,
			DeliveryDate = deliveryDate?.ToInstant(),
		};

		var id = _purchaseId ?? Guid.NewGuid(); // todo should this be saved?
		await GnomeshadeClient.PutPurchaseAsync(_transactionId, id, purchaseCreation).ConfigureAwait(false);
		return id;
	}
}
