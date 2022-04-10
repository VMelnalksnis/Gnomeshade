// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Purchases;

/// <summary>Overview of all <see cref="Purchase"/>s of a single <see cref="Transaction"/>.</summary>
public sealed class PurchaseViewModel : OverviewViewModel<PurchaseOverview, PurchaseUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Guid _transactionId;

	private PurchaseUpsertionViewModel _details;

	private PurchaseViewModel(
		IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		PurchaseUpsertionViewModel details)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_transactionId = transactionId;
		_details = details;

		PropertyChanged += OnPropertyChanged;
	}

	/// <inheritdoc />
	public override PurchaseUpsertionViewModel Details
	{
		get => _details;
		set => SetAndNotify(ref _details, value);
	}

	/// <summary>Gets the total purchased amount.</summary>
	public decimal Total => Rows.Select(overview => overview.Price).Sum();

	/// <summary>Initializes a new instance of the <see cref="PurchaseViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="transactionId">The transaction for which to create a purchase overview.</param>
	/// <returns>A new instance of the <see cref="PurchaseViewModel"/> class.</returns>
	public static async Task<PurchaseViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient, Guid transactionId)
	{
		var upsertionViewModel = await PurchaseUpsertionViewModel.CreateAsync(gnomeshadeClient, transactionId).ConfigureAwait(false);
		var viewModel = new PurchaseViewModel(gnomeshadeClient, transactionId, upsertionViewModel);
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

		var overviews = purchases.Select(purchase => purchase.ToOverview(currencies, products));

		Rows.CollectionChanged -= RowsOnCollectionChanged;
		Rows = new(overviews); // todo sorting
		Rows.CollectionChanged += RowsOnCollectionChanged;
	}

	/// <inheritdoc />
	public override async Task DeleteSelectedAsync()
	{
		if (Selected is null)
		{
			throw new InvalidOperationException();
		}

		await _gnomeshadeClient.DeletePurchaseAsync(_transactionId, Selected.Id).ConfigureAwait(false);
		await RefreshAsync();
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(Selected))
		{
			Details = PurchaseUpsertionViewModel.CreateAsync(_gnomeshadeClient, _transactionId, Selected?.Id).Result;
		}

		if (e.PropertyName is nameof(Rows))
		{
			OnPropertyChanged(nameof(Total));
		}
	}

	private void RowsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(Total));
	}
}
