// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Transactions.Transfers;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions;

/// <summary>Overview of all transaction schedules.</summary>
/// <seealso cref="TransactionSchedule"/>
public sealed partial class TransactionScheduleViewModel
	: OverviewViewModel<TransactionScheduleOverview, TransactionScheduleUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IDialogService _dialogService;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private TransactionScheduleUpsertionViewModel _details;

	/// <summary>Gets a collection of all the planned transactions for the <see cref="OverviewViewModel{TRow,TUpsertion}.Selected"/> schedule.</summary>
	[Notify(Setter.Private)]
	private List<TransactionOverview>? _plannedTransactions;

	/// <summary>Initializes a new instance of the <see cref="TransactionScheduleViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="dialogService">Service for creating dialog windows.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	public TransactionScheduleViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IDialogService dialogService,
		IDateTimeZoneProvider dateTimeZoneProvider)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_dialogService = dialogService;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_details = new(activityService, gnomeshadeClient, dialogService, dateTimeZoneProvider, null);

		_details.Upserted += DetailsOnUpserted;
	}

	/// <inheritdoc />
	public override TransactionScheduleUpsertionViewModel Details
	{
		get => _details;
		set
		{
			_details.Upserted -= DetailsOnUpserted;
			SetAndNotify(ref _details, value);
			_details.Upserted += DetailsOnUpserted;
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="TransferSummary.UserCurrency"/> column needs to be shown.</summary>
	public bool ShowUserCurrency => _plannedTransactions?
		.SelectMany(transaction => transaction.Transfers)
		.Any(transfer => !string.IsNullOrWhiteSpace(transfer.UserCurrency)) ?? false;

	/// <summary>
	/// Gets a value indicating whether the <see cref="TransferSummary.OtherCurrency"/> and
	/// <see cref="TransferSummary.OtherAmount"/> columns needs to be shown.
	/// </summary>
	public bool ShowOtherAmount => _plannedTransactions?
		.SelectMany(transaction => transaction.Transfers)
		.Any(transfer => transfer.DisplayTarget) ?? false;

	/// <inheritdoc />
	public override async Task UpdateSelection()
	{
		Details = new(ActivityService, _gnomeshadeClient, _dialogService, _dateTimeZoneProvider, Selected?.Id);
		await Details.RefreshAsync();

		if (Selected is null)
		{
			PlannedTransactions = null;
		}
		else
		{
			var currentTimeZone = _dateTimeZoneProvider.GetSystemDefault();
			await RefreshTransactions(currentTimeZone);
		}
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var schedules = await _gnomeshadeClient.GetTransactionSchedules();
		var currentTimeZone = _dateTimeZoneProvider.GetSystemDefault();
		var overviews = schedules
			.Select(schedule => new TransactionScheduleOverview(schedule, currentTimeZone))
			.ToArray();

		Rows = new(overviews);

		if (Selected is null)
		{
			await Details.RefreshAsync();
			_plannedTransactions = null;
		}
		else
		{
			await RefreshTransactions(currentTimeZone);
		}
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(TransactionScheduleOverview row)
	{
		await _gnomeshadeClient.DeleteProjectAsync(row.Id);
		Details = new(ActivityService, _gnomeshadeClient, _dialogService, _dateTimeZoneProvider, null);
	}

	private async void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync();
	}

	private async Task RefreshTransactions(DateTimeZone currentTimeZone)
	{
		var (counterparties, counterparty, accounts) = await (
			_gnomeshadeClient.GetCounterpartiesAsync(),
			_gnomeshadeClient.GetMyCounterpartyAsync(),
			_gnomeshadeClient.GetAccountsAsync())
			.WhenAll();

		var accountsInCurrency = accounts
			.SelectMany(account => account.Currencies.Select(currency => (AccountInCurrency: currency, Account: account)))
			.ToArray();

		var plannedTransactions = await _gnomeshadeClient.GetPlannedTransactions();
		var overviews = await Task.WhenAll(plannedTransactions
			.Select(async transaction =>
			{
				var plannedTransfers = await _gnomeshadeClient.GetPlannedTransfers(transaction.Id);
				var transfers = plannedTransfers
					.Select(plannedTransfer => plannedTransfer.ToSummary(plannedTransfer.BookedAt!.Value, counterparties, counterparty, accountsInCurrency))
					.ToList();

				var date = plannedTransfers
					.Select(transfer => transfer.BookedAt)
					.Max();

				var purchases = await _gnomeshadeClient.GetPlannedPurchases(transaction.Id);
				var loanPayments = await _gnomeshadeClient.GetPlannedLoanPayments(transaction.Id);

				return new TransactionOverview(
					transaction.Id,
					date!.Value.InZone(currentTimeZone).ToDateTimeOffset(),
					null,
					null,
					transfers,
					purchases,
					loanPayments,
					true);
			}));

		PlannedTransactions = overviews.ToList();
	}
}
