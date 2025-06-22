// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>Information needed to create as transfer.</summary>
/// <seealso cref="TransferBase"/>
public abstract record TransferCreationBase : TransactionItemCreation
{
	/// <inheritdoc cref="TransferBase.TransactionId"/>
	[Required]
	public override Guid? TransactionId { get; set; }

	/// <inheritdoc cref="TransferBase.SourceAmount"/>
	[Required]
	public decimal? SourceAmount { get; set; }

	/// <inheritdoc cref="Transfer.SourceAccountId"/>
	[Required]
	public abstract Guid? SourceAccountId { get; set; }

	/// <inheritdoc cref="TransferBase.TargetAmount"/>
	[Required]
	public decimal? TargetAmount { get; set; }

	/// <inheritdoc cref="Transfer.TargetAccountId"/>
	[Required]
	public abstract Guid? TargetAccountId { get; set; }

	/// <inheritdoc cref="TransferBase.Order"/>
	public uint? Order { get; set; }

	/// <inheritdoc cref="TransferBase.BookedAt"/>
	public abstract Instant? BookedAt { get; set; }
}
