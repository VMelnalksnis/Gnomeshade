// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using VMelnalksnis.ISO20022DotNet.MessageSets.BankToCustomerCashManagement.V2.AccountReport;
using VMelnalksnis.NordigenDotNet.Accounts;

namespace Gnomeshade.WebApi.V1.Importing;

/// <summary>Helper methods for translating Nordigen transaction data to common format.</summary>
public static class BookedTransactionExtensions
{
	/// <summary>Gets the ISO20022 compatible <see cref="CreditDebitCode"/> of the booked transaction.</summary>
	/// <param name="transaction">The transaction for which to get the code.</param>
	/// <returns>ISO20022 credit debit code of the transaction.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Could not determine the code for the transaction.</exception>
	public static CreditDebitCode GetCreditDebitCode(this BookedTransaction transaction) => transaction.AdditionalInformation switch
	{
		"PURCHASE" => CreditDebitCode.DBIT,
		"INWARD TRANSFER" => CreditDebitCode.CRDT,
		"INWARD CLEARING PAYMENT" => CreditDebitCode.CRDT,
		"INWARD INSTANT PAYMENT" => CreditDebitCode.CRDT,
		"RETURN OF PURCHASE" => CreditDebitCode.CRDT,
		"CARD FEE" => CreditDebitCode.DBIT,
		"BALANCE ENQUIRY FEE" => CreditDebitCode.DBIT,
		"OUTWARD TRANSFER" => CreditDebitCode.DBIT,
		"OUTWARD INSTANT PAYMENT" => CreditDebitCode.DBIT,
		"INTEREST PAYMENT" => CreditDebitCode.DBIT,
		"REIMBURSEMENT OF COMMISSION" => CreditDebitCode.DBIT,
		"PRINCIPAL REPAYMENT" => CreditDebitCode.DBIT,
		"CASH DEPOSIT" => CreditDebitCode.CRDT,
		"CASH WITHDRAWAL" => CreditDebitCode.DBIT,
		"LOAN DRAWDOWN" => CreditDebitCode.DBIT,
		var information when information?.StartsWith("INWARD", StringComparison.OrdinalIgnoreCase) ?? false => CreditDebitCode.CRDT,
		var information when information?.StartsWith("OUTWARD", StringComparison.OrdinalIgnoreCase) ?? false => CreditDebitCode.DBIT,
		_ => transaction.GetCode() switch
		{
			("PMNT", _, _) => CreditDebitCode.DBIT,

			// This will leak all data about the transaction into logs, but that should not be an issue while self-hosting
			// While only some fields are needed when this fails, those fields contain private information anyway
			_ => throw new ArgumentOutOfRangeException(
				nameof(transaction),
				transaction,
				"Failed to determine transaction type"),
		},
	};

	/// <summary>Gets the ISO20022 compatible payment codes of the booked transaction.</summary>
	/// <param name="transaction">The transaction for which to get the payment codes.</param>
	/// <returns>ISO2022 payment code of the transaction.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><see cref="Transaction.BankTransactionCode"/> does not contain a valid payment code.</exception>
	public static (string? Domain, string? Family, string? SubFamily) GetCode(this BookedTransaction transaction)
	{
		var bankTransactionCode = transaction.BankTransactionCode;
		if (string.IsNullOrWhiteSpace(bankTransactionCode))
		{
			return (null, null, null);
		}

		var codes = bankTransactionCode.Split('-');
		return codes switch
		{
			[var domain] => (domain, null, null),
			[var domain, var family] => (domain, family, null),
			[var domain, var family, var subFamily] => (domain, family, subFamily),
			_ => throw new ArgumentOutOfRangeException(
				nameof(transaction),
				transaction,
				"Unexpected bank transaction code structure"),
		};
	}
}
