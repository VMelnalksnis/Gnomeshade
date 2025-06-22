// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Gnomeshade.Data.Entities;

/// <summary>A <see cref="TransferEntity"/> that will happen in the future.</summary>
public sealed record PlannedTransferEntity : TransferBase
{
	/// <summary>Gets or sets the id of the account from which currency is withdrawn from.</summary>
	/// <seealso cref="AccountInCurrencyEntity"/>
	[MemberNotNullWhen(true, nameof(SourceUsesAccount))]
	public Guid? SourceAccountId { get; set; }

	/// <summary>Gets or sets the id of the counterparty from which currency will be withdrawn from.</summary>
	/// <seealso cref="CounterpartyEntity"/>
	[MemberNotNullWhen(false, nameof(SourceUsesAccount))]
	public Guid? SourceCounterpartyId { get; set; }

	/// <summary>Gets or sets the id of the currency in which funds will be withdrawn from <see cref="SourceCounterpartyId"/>.</summary>
	/// <seealso cref="CurrencyEntity"/>
	[MemberNotNullWhen(false, nameof(SourceUsesAccount))]
	public Guid? SourceCurrencyId { get; set; }

	/// <summary>Gets or sets the id of the <see cref="AccountInCurrencyEntity"/> to which currency is deposited to.</summary>
	/// <seealso cref="AccountInCurrencyEntity"/>
	[MemberNotNullWhen(true, nameof(TargetUsesAccount))]
	public Guid? TargetAccountId { get; set; }

	/// <summary>Gets or sets the id of the counterparty to which currency will be deposited to.</summary>
	/// <seealso cref="CounterpartyEntity"/>
	[MemberNotNullWhen(false, nameof(TargetUsesAccount))]
	public Guid? TargetCounterpartyId { get; set; }

	/// <summary>Gets or sets the id of the currency in which funds will be deposited to <see cref="TargetCounterpartyId"/>.</summary>
	/// <seealso cref="CurrencyEntity"/>
	[MemberNotNullWhen(false, nameof(TargetUsesAccount))]
	public Guid? TargetCurrencyId { get; set; }

	/// <summary>Gets a value indicating whether <see cref="SourceAccountId"/> or <see cref="SourceCounterpartyId"/> is specified.</summary>
	public bool SourceUsesAccount => SourceAccountId is not null;

	/// <summary>Gets a value indicating whether <see cref="TargetAccountId"/> or <see cref="TargetCounterpartyId"/> is specified.</summary>
	public bool TargetUsesAccount => TargetAccountId is not null;
}
