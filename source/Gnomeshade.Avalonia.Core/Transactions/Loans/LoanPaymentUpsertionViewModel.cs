﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Loans;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions.Loans;

/// <summary>Creates or updates a single loan payment.</summary>
public sealed partial class LoanPaymentUpsertionViewModel : UpsertionViewModel
{
	private readonly Guid _transactionId;

	/// <summary>Gets all available loans.</summary>
	[Notify(Setter.Private)]
	private List<Loan> _loans = [];

	/// <summary>Gets or sets the loan that this payment is a part of.</summary>
	[Notify]
	private Loan? _loan;

	/// <summary>Gets or sets the amount of the loan payment.</summary>
	/// <seealso cref="Currency"/>
	[Notify]
	private decimal? _amount;

	/// <summary>Gets or sets the interest amount of this loan payment.</summary>
	[Notify]
	private decimal? _interest;

	/// <summary>Initializes a new instance of the <see cref="LoanPaymentUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="transactionId">The id of the transaction to which to add the loan.</param>
	/// <param name="id">The id of the loan payment to edit.</param>
	public LoanPaymentUpsertionViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_transactionId = transactionId;
		Id = id;

		PropertyChanged += OnPropertyChanged;
	}

	/// <inheritdoc cref="AutoCompleteSelectors.Loan"/>
	public AutoCompleteSelector<object> LoanSelector => AutoCompleteSelectors.Loan;

	/// <inheritdoc />
	public override bool CanSave =>
		Loan is not null &&
		Amount is not null &&
		Interest is not null;

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		Loans = await GnomeshadeClient.GetLoansAsync();

		if (Id is not { } id)
		{
			return;
		}

		var payment = await GnomeshadeClient.GetLoanPaymentAsync(id);
		Loan = Loans.Single(loan => loan.Id == payment.LoanId);
		Amount = payment.Amount;
		Interest = payment.Interest;
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var creation = new LoanPaymentCreation
		{
			LoanId = Loan?.Id,
			TransactionId = _transactionId,
			Amount = Amount,
			Interest = Interest,
		};

		var id = Id ?? Guid.NewGuid();
		await GnomeshadeClient.PutLoanPaymentAsync(id, creation);
		return id;
	}

	private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not nameof(Loan) || Loan is not { } loan)
		{
			return;
		}

		var accounts = await GnomeshadeClient.GetAccountsAsync();
		var transaction = await GnomeshadeClient.GetDetailedTransactionAsync(_transactionId);

		var sourceCounterparties = transaction
			.Transfers
			.Select(transfer => accounts
				.Single(account => account.Currencies.Any(currency => currency.Id == transfer.SourceAccountId)))
			.Select(account => account.CounterpartyId)
			.Distinct()
			.ToArray();

		var targetCounterparties = transaction
			.Transfers
			.Select(transfer => accounts
				.Single(account => account.Currencies.Any(currency => currency.Id == transfer.TargetAccountId)))
			.Select(account => account.CounterpartyId)
			.Distinct()
			.ToArray();

		if (sourceCounterparties is not [var source] || targetCounterparties is not [var target])
		{
			return;
		}

		if (loan.IssuingCounterpartyId == source && loan.ReceivingCounterpartyId == target)
		{
			Amount = transaction.Transfers.Sum(transfer => transfer.SourceAmount);
			Interest = 0;
		}
		else if (loan.IssuingCounterpartyId == target && loan.ReceivingCounterpartyId == source)
		{
			if (transaction.Transfers.OrderByDescending(transfer => transfer.SourceAmount).ToArray() is [var amount, var interest])
			{
				Amount = -amount.SourceAmount;
				Interest = -interest.SourceAmount;
			}
			else if (transaction.Transfers is [var transfer])
			{
				Amount = -transfer.SourceAmount;
				Interest = 0;
			}
		}
	}
}
