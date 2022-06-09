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
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Loans;

/// <summary>Creates or updates a single loan.</summary>
public sealed class LoanUpsertionViewModel : UpsertionViewModel
{
	private readonly Guid _transactionId;

	private Guid? _id;
	private List<Counterparty> _counterparties;
	private List<Currency> _currencies;
	private Counterparty? _issuingCounterparty;
	private Counterparty? _receivingCounterparty;
	private decimal? _amount;
	private Currency? _currency;

	private LoanUpsertionViewModel(IGnomeshadeClient gnomeshadeClient, Guid transactionId, Guid? id)
		: base(gnomeshadeClient)
	{
		_transactionId = transactionId;
		_id = id;

		_counterparties = new();
		_currencies = new();
	}

	/// <summary>Gets a delegate for formatting a counterparty in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CounterpartySelector => AutoCompleteSelectors.Counterparty;

	/// <summary>Gets a delegate for formatting a currency in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CurrencySelector => AutoCompleteSelectors.Currency;

	/// <summary>Gets all available counterparties.</summary>
	public List<Counterparty> Counterparties
	{
		get => _counterparties;
		private set => SetAndNotify(ref _counterparties, value);
	}

	/// <summary>Gets all available currencies.</summary>
	public List<Currency> Currencies
	{
		get => _currencies;
		private set => SetAndNotify(ref _currencies, value);
	}

	/// <summary>Gets or sets the counterparty issuing the loan.</summary>
	/// <seealso cref="Counterparties"/>
	/// <seealso cref="CounterpartySelector"/>
	public Counterparty? IssuingCounterparty
	{
		get => _issuingCounterparty;
		set => SetAndNotifyWithGuard(ref _issuingCounterparty, value, nameof(IssuingCounterparty), CanSaveNames);
	}

	/// <summary>Gets or sets the counterparty receiving the loan.</summary>
	/// <seealso cref="Counterparties"/>
	/// <seealso cref="CounterpartySelector"/>
	public Counterparty? ReceivingCounterparty
	{
		get => _receivingCounterparty;
		set => SetAndNotifyWithGuard(ref _receivingCounterparty, value, nameof(ReceivingCounterparty), CanSaveNames);
	}

	/// <summary>Gets or sets the amount of the loan.</summary>
	/// <seealso cref="Currency"/>
	public decimal? Amount
	{
		get => _amount;
		set => SetAndNotifyWithGuard(ref _amount, value, nameof(Amount), CanSaveNames);
	}

	/// <summary>Gets or sets the currency of the loan.</summary>
	/// <seealso cref="Currencies"/>
	/// <seealso cref="CurrencySelector"/>
	public Currency? Currency
	{
		get => _currency;
		set => SetAndNotifyWithGuard(ref _currency, value, nameof(Currency), CanSaveNames);
	}

	/// <inheritdoc />
	public override bool CanSave =>
		IssuingCounterparty is not null &&
		ReceivingCounterparty is not null &&
		IssuingCounterparty.Id != ReceivingCounterparty.Id &&
		Amount is not null &&
		Currency is not null;

	/// <summary>Initializes a new instance of the <see cref="LoanUpsertionViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="transactionId">The id of the transaction to which to add the loan.</param>
	/// <param name="loanId">The id of the loan to edit.</param>
	/// <returns>A new instance of the <see cref="LoanUpsertionViewModel"/> class.</returns>
	public static async Task<LoanUpsertionViewModel> CreateAsync(
		IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid? loanId = null)
	{
		var viewModel = new LoanUpsertionViewModel(gnomeshadeClient, transactionId, loanId);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		Counterparties = await GnomeshadeClient.GetCounterpartiesAsync().ConfigureAwait(false);
		Currencies = await GnomeshadeClient.GetCurrenciesAsync().ConfigureAwait(false);

		if (_id is null)
		{
			return;
		}

		var loan = await GnomeshadeClient.GetLoanAsync(_transactionId, _id.Value);
		IssuingCounterparty = Counterparties.Single(counterparty => counterparty.Id == loan.IssuingCounterpartyId);
		ReceivingCounterparty = Counterparties.Single(counterparty => counterparty.Id == loan.ReceivingCounterpartyId);
		Amount = loan.Amount;
		Currency = Currencies.Single(currency => currency.Id == loan.CurrencyId);
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var creation = new LoanCreation
		{
			IssuingCounterpartyId = IssuingCounterparty?.Id,
			ReceivingCounterpartyId = ReceivingCounterparty?.Id,
			Amount = Amount,
			CurrencyId = Currency?.Id,
		};

		_id ??= Guid.NewGuid();
		await GnomeshadeClient.PutLoanAsync(_transactionId, _id.Value, creation).ConfigureAwait(false);
		return _id.Value;
	}
}
