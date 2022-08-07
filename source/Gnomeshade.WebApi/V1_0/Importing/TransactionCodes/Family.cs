// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Ardalis.SmartEnum;

using JetBrains.Annotations;

// ReSharper disable StringLiteralTypo

namespace Gnomeshade.WebApi.V1_0.Importing.TransactionCodes;

/// <summary>
/// Medium definition level: e.g. type of payments: credit transfer, direct debit.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
public class Family : SmartEnum<Family>
{
	/// <summary>
	/// The “Not Available” family is used to cater for the Bank Transaction Code mandatory field,
	/// when no further details are available for the Bank Transaction Code,
	/// eg a payment is reported but no family is available in the information provided in the transaction.
	/// </summary>
	public static readonly Family NotAvailable = new("NTAV", 1);

	/// <summary>
	/// The “Other” family is used to cater for the Bank Transaction Code mandatory field,
	/// when the reported family does not match any of the families listed in the specified domain,
	/// but further details are available in addition to the domain code.
	/// </summary>
	public static readonly Family Other = new("OTHR", 2);

	/// <summary>
	/// Transaction is related to miscellaneous credit operations on the balance or
	/// on a specific transaction on the account.
	/// </summary>
	public static readonly Family CreditOperation = new("MCOP", 3);

	/// <summary>
	/// Transaction is related to miscellaneous debit operations on the balance or
	/// on a specific transaction on the account.
	/// </summary>
	public static readonly Family DebitOperation = new("MDOP", 4);

	/// <summary>
	/// Receivable Credit Transfers are instructions to receive an amount of money from a debtor by the account owner.
	/// The receivable credit transfers are related to transactions received by the account owner.
	/// </summary>
	public static readonly Family ReceivedCreditTransfers = new("RCDT", 101);

	/// <summary>
	/// Payable Credit Transfers are instructions to transfer an amount of money by the account owner to a creditor.
	/// The payable credit transfers are related to instructions sent by the account owner.
	/// </summary>
	public static readonly Family IssuedCreditTransfers = new("ICDT", 102);

	/// <summary>
	/// Transaction is related to incoming cash movements that are related to cash management activities initiated
	/// by the owner of the sending account to optimise the return on the available funds.
	/// </summary>
	public static readonly Family ReceivedCashConcentrationTransactions = new("RCCN", 103);

	/// <summary>
	/// Transaction is related to outgoing cash movements that are related to cash management activities initiated
	/// by the owner of the account to optimise the return on the available funds.
	/// </summary>
	public static readonly Family IssuedCashConcentrationTransactions = new("ICCN", 104);

	/// <summary>
	/// The Received Direct Debit transactions are related to instructions received
	/// by the account owner to debit the account.
	/// </summary>
	public static readonly Family ReceivedDirectDebits = new("RDDT", 105);

	/// <summary>
	/// The Issued Direct Debit transactions are related to instructions sent
	/// by the account owner to collect an amount of money that is due to the account owner.
	/// </summary>
	public static readonly Family IssuedDirectDebits = new("IDDT", 106);

	/// <summary>
	/// Transaction is related to a written paper order – the cheque – received
	/// by the account owner from the cheque drawer, to credit the account of the owner.
	/// </summary>
	public static readonly Family ReceivedCheques = new("RCHQ", 107);

	/// <summary>
	/// Transaction is related to a written paper order – the cheque – issued
	/// by the account owner to the cheque recipient, to debit the account of the cheque issuer.
	/// </summary>
	public static readonly Family IssuedCheques = new("ICHQ", 108);

	/// <summary>
	/// Transaction is a payment card operation performed by the customer by the means of a debit or credit card.
	/// Cards are issued by a credit institution or a card company.
	/// They indicate that the holder of the card may charge his/her account at the bank (debit card) or
	/// draw on a line of credit (credit card) up to an authorised limit.
	/// </summary>
	public static readonly Family CustomerCardTransactions = new("CCRD", 109);

	/// <summary>
	/// Transaction is a payment card operation performed by debit or credit card operation, reported for the merchant.
	/// </summary>
	public static readonly Family MerchantCardTransactions = new("MCRD", 110);

	/// <summary>
	/// Transaction is related to a lockbox, which is a batch of cheques that have been deposited in a BO,
	/// and are processed in one operation.
	/// </summary>
	public static readonly Family LockboxTransactions = new("LBOX", 111);

	/// <summary>
	/// Transaction is related to cash movements initiated through
	/// over-the-counter operations at the financial institution’s counter.
	/// </summary>
	public static readonly Family CounterTransactions = new("CNTR", 112);

	/// <summary>
	/// Transaction is related to a guaranteed bank cheque issued by the account owner
	/// with a future value date (do not pay before), which in commercial terms is a ‘negotiable instrument’:
	/// the beneficiary can receive early payment from any bank under subtraction of a discount.
	/// The ordering customer’s account that has issued the draft is debited on value date.
	/// </summary>
	public static readonly Family Drafts = new("DRFT", 113);

	/// <summary>
	/// Receivable Real Time Credit Transfers are instructions to receive an amount of money from a debtor
	/// by the account owner in Real Time (within a pre-agreed number of seconds).
	/// The receivable credit transfers are related to transactions received by the account owner.
	/// </summary>
	public static readonly Family ReceivedRealTimeCreditTransfer = new("RRCT", 114);

	/// <summary>
	/// Payable Real Time Credit Transfers are instructions to transfer an amount of money
	/// by the account owner to a creditor in Real Time (within a pre-agreed number of seconds).
	/// The payable credit transfers are related to instructions sent by the account owner.
	/// </summary>
	public static readonly Family IssuedRealTimeCreditTransfer = new("IRCT", 115);

	private Family(string name, int value)
		: base(name, value)
	{
	}
}
