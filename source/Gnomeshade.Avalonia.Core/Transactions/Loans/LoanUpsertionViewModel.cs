// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Transactions;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions.Loans;

/// <summary>Creates or updates a single loan.</summary>
public sealed partial class LoanUpsertionViewModel : UpsertionViewModel
{
	private readonly Guid _transactionId;

	/// <summary>Gets all available counterparties.</summary>
	[Notify(Setter.Private)]
	private List<Counterparty> _counterparties;

	/// <summary>Gets all available currencies.</summary>
	[Notify(Setter.Private)]
	private List<Currency> _currencies;

	/// <summary>Gets or sets the counterparty issuing the loan.</summary>
	/// <seealso cref="Counterparties"/>
	/// <seealso cref="CounterpartySelector"/>
	[Notify]
	private Counterparty? _issuingCounterparty;

	/// <summary>Gets or sets the counterparty receiving the loan.</summary>
	/// <seealso cref="Counterparties"/>
	/// <seealso cref="CounterpartySelector"/>
	[Notify]
	private Counterparty? _receivingCounterparty;

	/// <summary>Gets or sets the amount of the loan.</summary>
	/// <seealso cref="Currency"/>
	[Notify]
	private decimal? _amount;

	/// <summary>Gets or sets the currency of the loan.</summary>
	/// <seealso cref="Currencies"/>
	/// <seealso cref="CurrencySelector"/>
	[Notify]
	private Currency? _currency;

	/// <summary>Initializes a new instance of the <see cref="LoanUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="transactionId">The id of the transaction to which to add the loan.</param>
	/// <param name="id">The id of the loan to edit.</param>
	public LoanUpsertionViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_transactionId = transactionId;
		Id = id;

		_counterparties = new();
		_currencies = new();
	}

	/// <summary>Gets a delegate for formatting a counterparty in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CounterpartySelector => AutoCompleteSelectors.Counterparty;

	/// <summary>Gets a delegate for formatting a currency in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CurrencySelector => AutoCompleteSelectors.Currency;

	/// <inheritdoc />
	public override bool CanSave =>
		IssuingCounterparty is not null &&
		ReceivingCounterparty is not null &&
		IssuingCounterparty.Id != ReceivingCounterparty.Id &&
		Amount is not null &&
		Currency is not null;

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		Counterparties = await GnomeshadeClient.GetCounterpartiesAsync();
		Currencies = await GnomeshadeClient.GetCurrenciesAsync();

		if (Id is null)
		{
			return;
		}

		var loan = await GnomeshadeClient.GetLoanAsync(Id.Value);
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
			TransactionId = _transactionId,
			IssuingCounterpartyId = IssuingCounterparty?.Id,
			ReceivingCounterpartyId = ReceivingCounterparty?.Id,
			Amount = Amount,
			CurrencyId = Currency?.Id,
		};

		var id = Id ?? Guid.NewGuid();
		await GnomeshadeClient.PutLoanAsync(id, creation);
		return id;
	}
}
