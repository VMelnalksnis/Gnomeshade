// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

public sealed record PlannedTransferEntity : Entity, IOwnableEntity, IModifiableEntity, ISortableEntity
{
	/// <inheritdoc />
	public Guid OwnerId { get; set; }

	/// <inheritdoc />
	public Instant ModifiedAt { get; set; }

	/// <inheritdoc />
	public Guid ModifiedByUserId { get; set; }

	/// <summary>Gets or sets the id of transaction this transfer is a part of.</summary>
	/// <seealso cref="TransactionEntity"/>
	public Guid PlannedTransactionId { get; set; }

	/// <summary>Gets or sets the amount withdrawn from the source account.</summary>
	public decimal SourceAmount { get; set; }

	/// <summary>Gets or sets the id of the account from which currency is withdrawn from.</summary>
	/// <seealso cref="AccountInCurrencyEntity"/>
	[MemberNotNullWhen(true, nameof(SourceUsesAccount))]
	public Guid? SourceAccountId { get; set; }

	/// <seealso cref="CounterpartyEntity"/>
	[MemberNotNullWhen(false, nameof(SourceUsesAccount))]
	public Guid? SourceCounterpartyId { get; set; }

	/// <seealso cref="CurrencyEntity"/>
	[MemberNotNullWhen(false, nameof(SourceUsesAccount))]
	public Guid? SourceCurrencyId { get; set; }

	/// <summary>Gets or sets the amount deposited in the target account.</summary>
	public decimal TargetAmount { get; set; }

	/// <summary>Gets or sets the id of the <see cref="AccountInCurrencyEntity"/> to which currency is deposited to.</summary>
	/// <seealso cref="AccountInCurrencyEntity"/>
	[MemberNotNullWhen(true, nameof(TargetUsesAccount))]
	public Guid? TargetAccountId { get; set; }

	/// <seealso cref="CounterpartyEntity"/>
	[MemberNotNullWhen(false, nameof(TargetUsesAccount))]
	public Guid? TargetCounterpartyId { get; set; }

	/// <seealso cref="CurrencyEntity"/>
	[MemberNotNullWhen(false, nameof(TargetUsesAccount))]
	public Guid? TargetCurrencyId { get; set; }

	/// <inheritdoc />
	public uint? Order { get; set; }

	public bool SourceUsesAccount => SourceAccountId is not null;

	public bool TargetUsesAccount => TargetAccountId is not null;
}
