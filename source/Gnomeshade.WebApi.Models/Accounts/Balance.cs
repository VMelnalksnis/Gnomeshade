// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.WebApi.Models.Accounts;

/// <summary>Account balance in a single currency.</summary>
public sealed record Balance
{
	/// <summary>The id of the <see cref="AccountInCurrency"/> for which the balance was calculated for.</summary>
	public Guid AccountInCurrencyId { get; set; }

	/// <summary>The total amount withdrawn from the account.</summary>
	public decimal SourceAmount { get; set; }

	/// <summary>The total amount deposited to the account.</summary>
	public decimal TargetAmount { get; set; }
}
