// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

namespace Gnomeshade.Avalonia.Core.Transactions.Loans.Migration;

/// <summary>Row for displaying how a loan will look after migrating.</summary>
public sealed class LoanMigrationRow : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="LoanMigrationRow"/> class.</summary>
	/// <param name="name">The name of the loan.</param>
	/// <param name="issuer">The issuer of the loan.</param>
	/// <param name="receiver">The receiver of the loan.</param>
	/// <param name="amounts">The amount of the loan payments.</param>
	/// <param name="currency">The currency code of <paramref name="amounts"/>.</param>
	public LoanMigrationRow(string name, string issuer, string receiver, List<decimal> amounts, string currency)
	{
		Name = name;
		Issuer = issuer;
		Receiver = receiver;
		Amounts = amounts;
		Currency = currency;
	}

	/// <summary>Gets the name of the loan.</summary>
	public string Name { get; }

	/// <summary>Gets the issuer of the loan.</summary>
	public string Issuer { get; }

	/// <summary>Gets the receiver of the loan.</summary>
	public string Receiver { get; }

	/// <summary>Gets the loan payment amounts.</summary>
	public List<decimal> Amounts { get; }

	/// <summary>Gets the currency code of <see cref="Amounts"/>.</summary>
	public string Currency { get; }
}
