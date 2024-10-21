// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Ardalis.SmartEnum;

#pragma warning disable CS1591

// ReSharper disable StringLiteralTypo

namespace Gnomeshade.WebApi.V1.Importing.TransactionCodes;

/// <summary>Lowest definition level: e.g. type of cheques: drafts, etc.</summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public sealed class SubFamily : SmartEnum<SubFamily>
{
	public static readonly SubFamily NotAvailable = new("NTAV", 1);
	public static readonly SubFamily Charges = new("CHRG", 2);
	public static readonly SubFamily Fees = new("FEES", 3);

	public static readonly SubFamily InternalBookTransfer = new("BOOK", 4);
	public static readonly SubFamily CashWithdrawl = new("CWDL", 5);
	public static readonly SubFamily SepaCreditTransfer = new("ESCT", 6);
	public static readonly SubFamily Interest = new("INTR", 7);

	public static readonly SubFamily DomesticCreditTransfer = new("DMCT", 10105);

	/// <summary>Transaction is an ATM deposit operation.</summary>
	public static readonly SubFamily CashDeposit = new("CDPT", 10902);

	public static readonly SubFamily PointOfSaleDebitCard = new("POSD", 10904);

	/// <summary>Transaction is related to drawdown of fixed term / notice / mortgage / consumer loans or syndications contracts.</summary>
	public static readonly SubFamily Drawdown = new("DDWN", 40101);

	/// <summary>Transaction is related to renewal of fixed term / notice / mortgage / consumer loans or syndications contracts.</summary>
	public static readonly SubFamily Renewal = new("RNEW", 40101);

	/// <summary>Transaction is related to the payment of the principal of fixed term / notice / mortgage / consumer loans or syndications contracts.</summary>
	public static readonly SubFamily PrincipalPayment = new("PPAY", 40101);

	private SubFamily(string name, int value)
		: base(name, value)
	{
	}

	internal static IEnumerable<SubFamily> BankSubFamilies { get; } = [Charges, Fees];
}
