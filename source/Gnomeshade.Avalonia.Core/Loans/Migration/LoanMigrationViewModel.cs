// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Loans;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Loans.Migration;

/// <summary>Overview of loans to be migrated to named loans/loan payments.</summary>
public sealed partial class LoanMigrationViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	/// <summary>Gets a collection of all current loans.</summary>
	[Notify(Setter.Private)]
	private List<LegacyLoanRow> _loans = [];

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

	/// <summary>Migrate legacy loans to loans/loan payments.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task MigrateLoans()
	{
		using var activity = BeginActivity("Migrating loans");

#pragma warning disable CS0612 // Type or member is obsolete
		var legacyLoans = await _gnomeshadeClient.GetLegacyLoans();
#pragma warning restore CS0612 // Type or member is obsolete
		var counterparties = await _gnomeshadeClient.GetCounterpartiesAsync();
		var transactions = await _gnomeshadeClient.GetDetailedTransactionsAsync(new(Instant.MinValue, Instant.MaxValue));
		var currencies = await _gnomeshadeClient.GetCurrenciesAsync();

		var loansToCreate = legacyLoans
			.OrderBy(loan =>
			{
				var transaction = transactions.Single(transaction => transaction.Id == loan.TransactionId);
				return transaction.ValuedAt ?? transaction.BookedAt;
			})
#pragma warning disable CS0612 // Type or member is obsolete
			.GroupBy(loan => loan, new LoanCounterpartyComparer())
#pragma warning restore CS0612 // Type or member is obsolete
			.Select(grouping =>
			{
				var counterparty1 =
					counterparties.Single(counterparty => counterparty.Id == grouping.Key.IssuingCounterpartyId);
				var counterparty2 =
					counterparties.Single(counterparty => counterparty.Id == grouping.Key.ReceivingCounterpartyId);
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

					var payments = grouping.Select(loan => new LoanPaymentCreation
					{
						TransactionId = loan.TransactionId,
						Amount = loan.Amount,
						Interest = 0,
					}).ToArray();

					var loanCreation = new LoanCreation
					{
						Name = name,
						IssuingCounterpartyId = issuer.Id,
						ReceivingCounterpartyId = receiver.Id,
						Principal = payments.Sum(payment => payment.Amount),
						CurrencyId = currency.Id,
					};

					return (loanCreation, payments);
				});
			});

		foreach (var (loanCreation, paymentCreations) in loansToCreate)
		{
			var loanId = await _gnomeshadeClient.CreateLoanAsync(loanCreation);
			foreach (var paymentCreation in paymentCreations)
			{
				await _gnomeshadeClient.CreateLoanPaymentAsync(paymentCreation with { LoanId = loanId });
			}
		}
	}

	/// <summary>Deletes all legacy loans.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task DeleteLegacyLoans()
	{
		using var activity = BeginActivity("Deleting legacy loans");

		foreach (var legacyLoanRow in Loans)
		{
#pragma warning disable CS0612 // Type or member is obsolete
			await _gnomeshadeClient.DeleteLegacyLoan(legacyLoanRow.Id);
#pragma warning restore CS0612 // Type or member is obsolete
		}
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
#pragma warning disable CS0612 // Type or member is obsolete
		var loans = await _gnomeshadeClient.GetLegacyLoans();
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
#pragma warning restore CS0612 // Type or member is obsolete

		Loans = loans
			.Select(loan => new LegacyLoanRow(loan, counterparties, currencies))
			.ToList();
	}
}
