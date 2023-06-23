// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions.Purchases;

/// <summary>Overview of all <see cref="Purchase"/>s of a single <see cref="Transaction"/>.</summary>
public sealed partial class PurchaseViewModel : OverviewViewModel<PurchaseOverview, PurchaseUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IDialogService _dialogService;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
	private readonly Guid _transactionId;

	private PurchaseUpsertionViewModel _details;

	/// <summary>Gets the total purchased amount.</summary>
	[Notify(Setter.Private)]
	private decimal _total;

	/// <summary>Initializes a new instance of the <see cref="PurchaseViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="dialogService">Service for creating dialog windows.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="transactionId">The transaction for which to create a purchase overview.</param>
	public PurchaseViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IDialogService dialogService,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid transactionId)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_dialogService = dialogService;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_transactionId = transactionId;
		_details = new(activityService, gnomeshadeClient, _dialogService, dateTimeZoneProvider, transactionId, null);

		PropertyChanged += OnPropertyChanged;
		_details.Upserted += DetailsOnUpserted;
	}

	/// <inheritdoc />
	public override PurchaseUpsertionViewModel Details
	{
		get => _details;
		set
		{
			_details.Upserted -= DetailsOnUpserted;
			SetAndNotify(ref _details, value);
			_details.Upserted += DetailsOnUpserted;
		}
	}

	/// <inheritdoc />
	public override async Task UpdateSelection()
	{
		Details = new(ActivityService, _gnomeshadeClient, _dialogService, _dateTimeZoneProvider, _transactionId, Selected?.Id);
		await Details.RefreshAsync();
		await SetDefaultCurrency();
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var transaction = await _gnomeshadeClient.GetDetailedTransactionAsync(_transactionId);

		var currenciesTask = _gnomeshadeClient.GetCurrenciesAsync();
		var productIds = transaction.Purchases.Select(purchase => purchase.ProductId).Distinct();
		var productsTask = Task.WhenAll(productIds.Select(async id => await _gnomeshadeClient.GetProductAsync(id)));
		var unitsTask = _gnomeshadeClient.GetUnitsAsync();

		await Task.WhenAll(productsTask, currenciesTask, unitsTask);
		var currencies = currenciesTask.Result;
		var products = productsTask.Result;
		var units = unitsTask.Result;

		var overviews = transaction.Purchases
			.OrderBy(purchase => purchase.Order)
			.ThenBy(purchase => purchase.ModifiedAt)
			.Select(purchase => purchase.ToOverview(currencies, products, units, _dateTimeZoneProvider));

		var sort = DataGridView.SortDescriptions;
		var selected = Selected;

		IsReadOnly = transaction.Reconciled;
		Total = transaction.PurchaseTotal;
		Rows.CollectionChanged -= RowsOnCollectionChanged;
		Rows = new(overviews);
		Rows.CollectionChanged += RowsOnCollectionChanged;
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = Rows.SingleOrDefault(overview => overview.Id == selected?.Id);

		if (Selected is null)
		{
			await Details.RefreshAsync();
			await SetDefaultCurrency();
		}
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(PurchaseOverview row)
	{
		await _gnomeshadeClient.DeletePurchaseAsync(row.Id);
		await RefreshAsync();
	}

	private async Task SetDefaultCurrency()
	{
		var currencyNames = Rows.Select(overview => overview.CurrencyName).Distinct().ToList();
		if (currencyNames.Count > 1)
		{
			return;
		}

		var currencyName = currencyNames.FirstOrDefault();
		var transfers = await _gnomeshadeClient.GetTransfersAsync(_transactionId);
		if (string.IsNullOrWhiteSpace(currencyName))
		{
			var accounts =
				(await _gnomeshadeClient.GetAccountsAsync()).SelectMany(account =>
					account.Currencies);
			var c = transfers
				.Select(transfer => transfer.SourceAccountId)
				.Concat(transfers.Select(transfer => transfer.TargetAccountId))
				.Select(id => accounts.Single(ac => ac.Id == id).CurrencyAlphabeticCode)
				.Distinct()
				.ToList();

			if (c.Count == 1)
			{
				currencyName = c.Single();
			}
			else
			{
				return;
			}
		}

		Details.Currency = Details.Currencies.Single(currency => currency.AlphabeticCode == currencyName);
		Details.Price ??= transfers.Sum(transfer => transfer.SourceAmount) - Rows.Sum(row => row.Price);

		var deliveryDate = Rows.Select(purchase => purchase.DeliveryDate).LastOrDefault(date => date is not null);
		Details.DeliveryDate = deliveryDate?.Date;
		Details.DeliveryTime = deliveryDate?.TimeOfDay;

		var lastOrder = Rows.Select(purchase => purchase.Order).Max() ?? default;
		Details.Order ??= lastOrder + 1;
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(Rows))
		{
			OnPropertyChanged(nameof(Total));
		}
	}

	private void RowsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(Total));
	}

	private async void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync();
	}
}
