// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core.Products;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions.Purchases;

/// <summary>Create or update a purchase.</summary>
public sealed partial class PurchaseUpsertionViewModel : UpsertionViewModel
{
	private readonly IDialogService _dialogService;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
	private readonly Guid _transactionId;
	private readonly Guid? _id;

	/// <summary>Gets or sets the amount paid to purchase an <see cref="Amount"/> of <see cref="Product"/>.</summary>
	[Notify]
	private decimal? _price;

	/// <summary>Gets or sets the id of the currency of <see cref="Price"/>.</summary>
	[Notify]
	private Currency? _currency;

	/// <summary>Gets or sets the id of the purchased product.</summary>
	[Notify]
	private Product? _product;

	/// <summary>Gets or sets the amount of <see cref="Product"/> that was purchased.</summary>
	[Notify]
	private decimal? _amount;

	/// <summary>Gets or sets the date when the <see cref="Product"/> was delivered.</summary>
	[Notify]
	private DateTimeOffset? _deliveryDate;

	/// <summary>Gets or sets the time when the <see cref="Product"/> was delivered.</summary>
	[Notify]
	private TimeSpan? _deliveryTime;

	/// <summary>Gets a collection of all currencies.</summary>
	[Notify(Setter.Private)]
	private List<Currency> _currencies;

	/// <summary>Gets a collection of all products.</summary>
	[Notify(Setter.Private)]
	private List<Product> _products;

	/// <summary>Gets the name of the unit of the <see cref="Product"/>.</summary>
	[Notify(Setter.Private)]
	private string? _unitName;

	/// <summary>Gets or sets the order of the purchase within a transaction.</summary>
	[Notify]
	private uint? _order;

	/// <summary>Initializes a new instance of the <see cref="PurchaseUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="dialogService">Service for creating dialog windows.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="transactionId">The id of the transaction to which to add the purchase to.</param>
	/// <param name="id">The id of the purchase to edit.</param>
	public PurchaseUpsertionViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IDialogService dialogService,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid transactionId,
		Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_dialogService = dialogService;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_transactionId = transactionId;
		_id = id;
		_currencies = new();
		_products = new();

		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Gets a delegate for formatting a currency in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CurrencySelector => AutoCompleteSelectors.Currency;

	/// <summary>Gets a delegate for formatting a product in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> ProductSelector => AutoCompleteSelectors.Product;

	/// <inheritdoc />
	public override bool CanSave =>
		Price is not null &&
		Currency is not null &&
		Product is not null &&
		Amount is not null;

	/// <summary>Shows a modal dialog for creating a new product.</summary>
	/// <param name="window">The current window.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task ShowNewProductDialog(Window window)
	{
		using var activity = BeginActivity("Waiting for product creation");
		var viewModel = new ProductUpsertionViewModel(ActivityService, GnomeshadeClient, _dateTimeZoneProvider, null);
		await viewModel.RefreshAsync();

		var result = await _dialogService.ShowDialogValue<ProductUpsertionViewModel, Guid>(window, viewModel, dialog =>
		{
			dialog.Title = "Create product";
			viewModel.Upserted += (_, args) => dialog.Close(args.Id);
		});

		await RefreshAsync();
		if (result is not { } createdId)
		{
			return;
		}

		Product = Products.SingleOrDefault(product => product.Id == createdId);
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		if (_id is null)
		{
			Currencies = await GnomeshadeClient.GetCurrenciesAsync();
			Products = await GnomeshadeClient.GetProductsAsync();
		}
		else
		{
			Currencies = await GnomeshadeClient.GetCurrenciesAsync();
			Products = await GnomeshadeClient.GetProductsAsync();
			var purchase = await GnomeshadeClient.GetPurchaseAsync(_id.Value);
			Price = purchase.Price;
			Currency = Currencies.Single(currency => currency.Id == purchase.CurrencyId);
			Amount = purchase.Amount;
			Product = Products.Single(product => product.Id == purchase.ProductId);
			DeliveryDate = purchase.DeliveryDate?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset();
			DeliveryTime = purchase.DeliveryDate?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset()
				.TimeOfDay;
			Order = purchase.Order;
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
			TransactionId = _transactionId,
			Price = Price,
			CurrencyId = Currency!.Id,
			Amount = Amount,
			ProductId = Product!.Id,
			DeliveryDate = deliveryDate?.ToInstant(),
			Order = Order,
		};

		var id = _id ?? Guid.NewGuid(); // todo should this be saved?
		await GnomeshadeClient.PutPurchaseAsync(id, purchaseCreation);
		return id;
	}

	private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(Product))
		{
			UnitName = Product?.UnitId is null
				? null
				: (await GnomeshadeClient.GetUnitAsync(Product.UnitId.Value)).Name;
		}
	}
}
