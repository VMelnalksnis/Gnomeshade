// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Loans;
using Gnomeshade.WebApi.Client;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions.Loans;

/// <summary>Overview of all loan payments for a single transaction.</summary>
public sealed partial class LoanPaymentViewModel : OverviewViewModel<LoanPaymentRow, LoanPaymentUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Guid _transactionId;
	private LoanPaymentUpsertionViewModel _details;

	/// <summary>Gets the total loaned amount.</summary>
	[Notify(Setter.Private)]
	private decimal _total;

	/// <summary>Initializes a new instance of the <see cref="LoanPaymentViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="transactionId">The transaction for which to create a loan overview.</param>
	public LoanPaymentViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient, Guid transactionId)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_transactionId = transactionId;
		_details = new(activityService, gnomeshadeClient, transactionId, null);

		PropertyChanged += OnPropertyChanged;
		_details.Upserted += DetailsOnUpserted;
	}

	/// <inheritdoc />
	public override LoanPaymentUpsertionViewModel Details
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
		Details = new(ActivityService, _gnomeshadeClient, _transactionId, Selected?.Id);
		await Details.RefreshAsync();
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var transaction = await _gnomeshadeClient.GetDetailedTransactionAsync(_transactionId);
		var payments = await _gnomeshadeClient.GetLoanPaymentsForTransactionAsync(_transactionId);
		var loans = await _gnomeshadeClient.GetLoansAsync();
		var currencies = await _gnomeshadeClient.GetCurrenciesAsync();

		var overviews = payments
			.Select(payment => new LoanPaymentRow(payment, loans, currencies))
			.ToList();

		var selected = Selected;
		IsReadOnly = transaction.Reconciled;
		Total = transaction.LoanTotal;
		Rows = new(overviews);
		Selected = Rows.SingleOrDefault(row => row.Id == selected?.Id);

		if (Selected is null)
		{
			await Details.RefreshAsync();
		}
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(LoanPaymentRow row)
	{
		await _gnomeshadeClient.DeleteLoanPaymentAsync(row.Id);
		await RefreshAsync();
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(Rows))
		{
			OnPropertyChanged(nameof(Total));
		}
	}

	private async void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync();
	}
}
