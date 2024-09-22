// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>Information needed to create a planned transfer.</summary>
/// <seealso cref="PlannedTransfer"/>
[PublicAPI]
public sealed record PlannedTransferCreation : TransferCreationBase
{
	/// <inheritdoc cref="TransferBase.TransactionId"/>
	[Required]
	public override Guid? TransactionId { get; set; }

	/// <inheritdoc cref="PlannedTransfer.SourceAccountId"/>
	[RequiredIfNull(nameof(SourceCounterpartyId))]
	public override Guid? SourceAccountId { get; set; }

	/// <inheritdoc cref="PlannedTransfer.SourceCounterpartyId"/>
	[RequiredIfNull(nameof(SourceAccountId))]
	public Guid? SourceCounterpartyId { get; set; }

	/// <inheritdoc cref="PlannedTransfer.SourceCurrencyId"/>
	[RequiredIfNotNull(nameof(SourceCounterpartyId))]
	public Guid? SourceCurrencyId { get; set; }

	/// <inheritdoc cref="PlannedTransfer.TargetAccountId"/>
	[RequiredIfNull(nameof(TargetCounterpartyId))]
	public override Guid? TargetAccountId { get; set; }

	/// <inheritdoc cref="PlannedTransfer.TargetCounterpartyId"/>
	[RequiredIfNull(nameof(TargetAccountId))]
	public Guid? TargetCounterpartyId { get; set; }

	/// <inheritdoc cref="PlannedTransfer.TargetCurrencyId"/>
	[RequiredIfNotNull(nameof(TargetCounterpartyId))]
	public Guid? TargetCurrencyId { get; set; }

	/// <inheritdoc cref="TransferBase.BookedAt"/>
	[Required]
	public override Instant? BookedAt { get; set; }
}
