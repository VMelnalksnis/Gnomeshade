// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Transactions.Transfers;

/// <summary>Overview of all planned transfers for a single <see cref="Transaction"/>.</summary>
public sealed class PlannedTransferViewModel : OverviewViewModel<TransferOverview, PlannedTransferUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IDialogService _dialogService;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
	private readonly Guid _transactionId;

	private PlannedTransferUpsertionViewModel _details;

	/// <summary>Initializes a new instance of the <see cref="PlannedTransferViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="dialogService">Service for creating dialog windows.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="transactionId">The transaction for which to create a planned transfer overview.</param>
	public PlannedTransferViewModel(
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
		_details = new(activityService, gnomeshadeClient, _dialogService, _dateTimeZoneProvider, transactionId, null);

		_details.Upserted += DetailsOnUpserted;
	}

	/// <inheritdoc />
	public override PlannedTransferUpsertionViewModel Details
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
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var (transfers, accounts, counterparties, currencies) = await (
			_gnomeshadeClient.GetPlannedTransfers(_transactionId),
			_gnomeshadeClient.GetAccountsAsync(),
			_gnomeshadeClient.GetCounterpartiesAsync(),
			_gnomeshadeClient.GetCurrenciesAsync())
			.WhenAll();
		var zone = _dateTimeZoneProvider.GetSystemDefault();

		Rows = new(transfers.Select(transfer => transfer.ToOverview(accounts, counterparties, currencies, zone)));

		Details = new(ActivityService, _gnomeshadeClient, _dialogService, _dateTimeZoneProvider, _transactionId, Selected?.Id);
		await Details.RefreshAsync();
	}

	/// <inheritdoc />
	protected override Task DeleteAsync(TransferOverview row) => throw new NotImplementedException();

	private async void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync();
	}
}
