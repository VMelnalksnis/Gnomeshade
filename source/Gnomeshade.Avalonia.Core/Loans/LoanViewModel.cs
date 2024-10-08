// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Loans;

/// <summary>An overview of all loans.</summary>
public sealed class LoanViewModel : OverviewViewModel<LoanRow, LoanUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private LoanUpsertionViewModel _details;

	/// <summary>Initializes a new instance of the <see cref="LoanViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	public LoanViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IDateTimeZoneProvider dateTimeZoneProvider)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_dateTimeZoneProvider = dateTimeZoneProvider;

		_details = new(activityService, _gnomeshadeClient, dateTimeZoneProvider, null);
	}

	/// <inheritdoc />
	public override LoanUpsertionViewModel Details
	{
		get => _details;
		set => SetAndNotify(ref _details, value);
	}

	/// <inheritdoc />
	public override async Task UpdateSelection()
	{
		Details = new(ActivityService, _gnomeshadeClient, _dateTimeZoneProvider, Selected?.Id);
		await Details.RefreshAsync();
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(LoanRow row)
	{
		await _gnomeshadeClient.DeleteLoanAsync(row.Id);
		Details = new(ActivityService, _gnomeshadeClient, _dateTimeZoneProvider, null);
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var (loans, loanPayments, counterparties, currencies) = await (
			_gnomeshadeClient.GetLoansAsync(),
			_gnomeshadeClient.GetLoanPaymentsAsync(),
			_gnomeshadeClient.GetCounterpartiesAsync(),
			_gnomeshadeClient.GetCurrenciesAsync())
			.WhenAll();

		var loanOverviews = loans
			.Select(loan => new LoanRow(loan, counterparties, currencies, loanPayments))
			.ToArray();

		Rows = new(loanOverviews);

		Details = new(ActivityService, _gnomeshadeClient, _dateTimeZoneProvider, Selected?.Id);
		await Details.RefreshAsync();
	}
}
