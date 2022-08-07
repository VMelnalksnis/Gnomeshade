// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Ardalis.SmartEnum;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.V1_0.Importing.TransactionCodes;

/// <summary>
/// Highest definition level to identify the sub-ledger.
/// The domain defines the business area of the underlying transaction (e.g., payments, securities...).
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
public sealed class Domain : SmartEnum<Domain>
{
	/// <summary>
	/// The Payments domain provides the bank transaction codes for all payment activities
	/// that relate to transfer of funds between parties.
	/// </summary>
	public static readonly Domain Payments = new("PMNT", 1);

	/// <summary>
	/// The Cash Management domain provides the bank transaction codes for cash management activities
	/// that relate to own account management, i.e. cash concentration, zero-balancing or
	/// topping of accounts or pooling activities. The underlying objective of these techniques
	/// is the coverage of funds deficits in one area with surpluses in another.
	/// </summary>
	public static readonly Domain CashManagement = new("CAMT", 2);

	/// <summary>
	/// The Derivatives domain provides the bank transaction codes for the derivatives related transactions,
	/// i.e. a financial instrument derived from a cash market commodity, futures contract, or other
	/// financial instrument. Derivatives can be traded on regulated exchanges as listed derivatives or over-the-counter.
	/// </summary>
	public static readonly Domain Derivatives = new("DERV", 3);

	/// <summary>
	/// The Loans, Deposits and Syndications domain provides the bank transaction codes of all operations that
	/// are related to loans, deposits and syndications management.
	/// </summary>
	public static readonly Domain LoansAndDeposits = new("LDAS", 4);

	/// <summary>
	/// The Foreign Exchange domain provides the bank transaction codes of all operations that are related to
	/// the foreign exchange market. Often abbreviated as FOREX.
	/// </summary>
	public static readonly Domain ForeignExchange = new("FORX", 5);

	/// <summary>
	/// The Precious Metal domain provides the bank transaction codes of all operations that are related to
	/// a classification of metals that are considered to be rare and/or have a high economic value.
	/// </summary>
	public static readonly Domain PreciousMetal = new("PMET", 6);

	/// <summary>
	/// The Commodities domain provides the bank transaction codes of all operations that are related to
	/// a commodity which might be an extraction (mining), an agricultural product (soybeans, grains, coffee, etc.),
	/// a non-precious metal, wood, or any other physical substance that investors buy or sell,
	/// usually as commodity futures contracts. They are complex, and include a wide array of instruments
	/// to manage risk through contracts for delivery of any product or service
	/// that can be characterized in an interchangeable way.
	/// </summary>
	public static readonly Domain Commodities = new("CMDT", 7);

	/// <summary>
	/// The Trade Services domain provides the bank transaction codes related to
	/// all of the Trade Services operations that need to be reported in the statements.
	/// </summary>
	public static readonly Domain TradeServices = new("TRAD", 8);

	/// <summary>
	/// The Securities domain provides the bank transaction codes for cash movements related to
	/// transactions on equities, fixed income and other securities industry related financial instruments.
	/// </summary>
	public static readonly Domain Securities = new("SECU", 9);

	/// <summary>
	/// The Account Management domain provides the bank transaction codes for operations on one account.
	/// Those transactions imply cash movements related to activities between
	/// the financial institution servicing the account and the customer/owner of the account.
	/// </summary>
	public static readonly Domain AccountManagement = new("ACMT", 10);

	/// <summary>
	/// The extended domain code is to be used whenever a specific domain has not yet been identified,
	/// or a proprietary Bank Transaction Code has not been associated with a specific domain.
	/// </summary>
	public static readonly Domain Extended = new("XTND", 11);

	private Domain(string name, int value)
		: base(name, value)
	{
	}
}
