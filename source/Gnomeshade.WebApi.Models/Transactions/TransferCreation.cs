// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>Information needed to create a transfer.</summary>
/// <seealso cref="Transfer"/>
[PublicAPI]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code")]
public sealed record TransferCreation : TransactionItemCreation
{
	/// <inheritdoc cref="Transfer.TransactionId"/>
	[Required]
	public override Guid? TransactionId { get; set; }

	/// <inheritdoc cref="Transfer.SourceAmount"/>
	[Required]
	public decimal? SourceAmount { get; set; }

	/// <inheritdoc cref="Transfer.SourceAccountId"/>
	[Required]
	public Guid? SourceAccountId { get; set; }

	/// <inheritdoc cref="Transfer.TargetAmount"/>
	[Required]
	public decimal? TargetAmount { get; set; }

	/// <inheritdoc cref="Transfer.TargetAccountId"/>
	[Required]
	public Guid? TargetAccountId { get; set; }

	/// <inheritdoc cref="Transfer.BankReference"/>
	public string? BankReference { get; set; }

	/// <inheritdoc cref="Transfer.ExternalReference"/>
	public string? ExternalReference { get; set; }

	/// <inheritdoc cref="Transfer.InternalReference"/>
	public string? InternalReference { get; set; }

	/// <inheritdoc cref="Transfer.Order"/>
	public uint? Order { get; set; }

	/// <inheritdoc cref="Transfer.BookedAt"/>
	[RequiredIfNull(nameof(ValuedAt))]
	public Instant? BookedAt { get; set; }

	/// <inheritdoc cref="Transfer.ValuedAt"/>
	[RequiredIfNull(nameof(BookedAt))]
	public Instant? ValuedAt { get; set; }
}
