// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

using Gnomeshade.WebApi.Models.Accounts;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>A planned transfer between two accounts.</summary>
/// <seealso cref="PlannedTransferCreation"/>
[PublicAPI]
public sealed record PlannedTransfer : TransferBase
{
	/// <summary>The id of the account from which currency is withdrawn from.</summary>
	/// <seealso cref="AccountInCurrency"/>
	public Guid? SourceAccountId { get; set; }

	/// <summary>Whether <see cref="SourceAccountId"/> or <see cref="SourceCounterpartyId"/> is specified.</summary>
	[MemberNotNullWhen(true, nameof(SourceAccountId))]
	[MemberNotNullWhen(false, nameof(SourceCounterpartyId))]
	[MemberNotNullWhen(false, nameof(SourceCurrencyId))]
	public bool IsSourceAccount => SourceAccountId.HasValue;

	/// <summary>The id of the counterparty from which currency will be withdrawn from.</summary>
	public Guid? SourceCounterpartyId { get; set; }

	/// <summary>The id of the currency in which funds will be withdrawn from <see cref="SourceCounterpartyId"/>.</summary>
	public Guid? SourceCurrencyId { get; set; }

	/// <summary>The id of the account to which currency is deposited to.</summary>
	/// <seealso cref="AccountInCurrency"/>
	public Guid? TargetAccountId { get; set; }

	/// <summary>Whether <see cref="TargetAccountId"/> or <see cref="TargetCounterpartyId"/> is specified.</summary>
	[MemberNotNullWhen(true, nameof(TargetAccountId))]
	[MemberNotNullWhen(false, nameof(TargetCounterpartyId))]
	[MemberNotNullWhen(false, nameof(TargetCurrencyId))]
	public bool IsTargetAccount => TargetAccountId.HasValue;

	/// <summary>The id of the counterparty to which currency will be deposited to.</summary>
	public Guid? TargetCounterpartyId { get; set; }

	/// <summary>The id of the currency in which funds will be deposited to <see cref="TargetCounterpartyId"/>.</summary>
	public Guid? TargetCurrencyId { get; set; }
}
