// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Data.Entities;

/// <summary>Account balance in a single currency.</summary>
public sealed class BalanceEntity
{
	/// <summary>Gets the id of the <see cref="AccountInCurrencyEntity"/> for which the balance was calculated for.</summary>
	public Guid AccountInCurrencyId { get; init; }

	/// <summary>Gets the total amount withdrawn from the account.</summary>
	public decimal SourceAmount { get; init; }

	/// <summary>Gets the total amount deposited to the account.</summary>
	public decimal TargetAmount { get; init; }
}
