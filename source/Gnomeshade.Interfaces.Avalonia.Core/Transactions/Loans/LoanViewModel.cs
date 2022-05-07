﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Loans;

/// <summary>Overview of all loans for a single transaction.</summary>
public sealed class LoanViewModel : OverviewViewModel<LoanOverview, LoanUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Guid _transactionId;
	private LoanUpsertionViewModel _details;

	private LoanViewModel(IGnomeshadeClient gnomeshadeClient, Guid transactionId, LoanUpsertionViewModel details)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_transactionId = transactionId;
		_details = details;

		PropertyChanged += OnPropertyChanged;
	}

	/// <inheritdoc />
	public override LoanUpsertionViewModel Details
	{
		get => _details;
		set => SetAndNotify(ref _details, value);
	}

	/// <summary>Gets the total loaned amount.</summary>
	public decimal Total => Rows.Select(overview => overview.Amount).Sum();

	/// <summary>Initializes a new instance of the <see cref="LoanViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="transactionId">The transaction for which to create a loan overview.</param>
	/// <returns>A new instance of the <see cref="LoanViewModel"/> class.</returns>
	public static async Task<LoanViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient, Guid transactionId)
	{
		var upsertionViewModel = await LoanUpsertionViewModel.CreateAsync(gnomeshadeClient, transactionId).ConfigureAwait(false);
		var viewModel = new LoanViewModel(gnomeshadeClient, transactionId, upsertionViewModel);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <inheritdoc />
	public override async Task RefreshAsync()
	{
		var loans = await _gnomeshadeClient.GetLoansAsync(_transactionId).ConfigureAwait(false);
		var counterparties = await _gnomeshadeClient.GetCounterpartiesAsync().ConfigureAwait(false);
		var currencies = await _gnomeshadeClient.GetCurrenciesAsync().ConfigureAwait(false);
		var overviews = loans
			.Select(loan => new LoanOverview(
				loan.Id,
				counterparties.Single(counterparty => counterparty.Id == loan.IssuingCounterpartyId).Name,
				counterparties.Single(counterparty => counterparty.Id == loan.ReceivingCounterpartyId).Name,
				loan.Amount,
				currencies.Single(currency => currency.Id == loan.CurrencyId).AlphabeticCode))
			.ToList();

		Rows = new(overviews);
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(LoanOverview row)
	{
		await _gnomeshadeClient.DeleteLoanAsync(_transactionId, row.Id).ConfigureAwait(false);
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(Rows))
		{
			OnPropertyChanged(nameof(Total));
		}
	}
}