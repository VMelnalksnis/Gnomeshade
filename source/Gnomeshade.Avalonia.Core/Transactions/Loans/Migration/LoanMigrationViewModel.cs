// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions.Loans.Migration;

/// <summary>Overview of loans to be migrated to named loans/loan payments.</summary>
public sealed partial class LoanMigrationViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	/// <summary>Gets a collection of all current loans.</summary>
	[Notify(Setter.Private)]
	private List<LoanOverview> _loans = [];

	/// <summary>Gets a collection of all loan payments that will be created.</summary>
	[Notify(Setter.Private)]
	private List<LoanMigrationRow> _migratedLoans = [];

	/// <summary>Initializes a new instance of the <see cref="LoanMigrationViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	public LoanMigrationViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var loans = await _gnomeshadeClient.GetLoansAsync();
		var counterparties = await _gnomeshadeClient.GetCounterpartiesAsync();
		var transactions = await _gnomeshadeClient.GetDetailedTransactionsAsync(new(Instant.MinValue, Instant.MaxValue));
		var currencies = await _gnomeshadeClient.GetCurrenciesAsync();

		MigratedLoans = loans
			.OrderBy(loan =>
			{
				var transaction = transactions.Single(transaction => transaction.Id == loan.TransactionId);
				return transaction.ValuedAt ?? transaction.BookedAt;
			})
			.GroupBy(loan => loan, new LoanCounterpartyComparer())
			.Select(grouping =>
			{
				var counterparty1 = counterparties.Single(counterparty => counterparty.Id == grouping.Key.IssuingCounterpartyId);
				var counterparty2 = counterparties.Single(counterparty => counterparty.Id == grouping.Key.ReceivingCounterpartyId);
				return (counterparty1, counterparty2, grouping.ToArray());
			})
			.SelectMany(loanTuple =>
			{
				var issuer = loanTuple.Item3.First().IssuingCounterpartyId == loanTuple.counterparty1.Id
					? loanTuple.counterparty1
					: loanTuple.counterparty2;

				var receiver = loanTuple.counterparty1 == issuer
					? loanTuple.counterparty2
					: loanTuple.counterparty1;

				return loanTuple.Item3.GroupBy(loan => loan.CurrencyId).Select(grouping =>
				{
					var currency = currencies.Single(currency => currency.Id == grouping.Key);
					var name = $"{issuer.Name} loan to {receiver.Name} ({currency.AlphabeticCode})";
					return new LoanMigrationRow(name, issuer.Name, receiver.Name, grouping.Select(loan => loan.Amount).ToList(), currency.AlphabeticCode);
				});
			})
			.ToList();

		Loans = transactions
			.SelectMany(transaction => transaction.Loans)
			.Select(loan => new LoanOverview(
				loan.Id,
				counterparties.Single(counterparty => counterparty.Id == loan.IssuingCounterpartyId).Name,
				counterparties.Single(counterparty => counterparty.Id == loan.ReceivingCounterpartyId).Name,
				loan.Amount,
				currencies.Single(currency => currency.Id == loan.CurrencyId).AlphabeticCode))
			.ToList();
	}
}
