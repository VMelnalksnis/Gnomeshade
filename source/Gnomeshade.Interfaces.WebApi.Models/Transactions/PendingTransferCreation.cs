// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Transactions;

/// <summary>Information needed to create a pending transfer.</summary>
/// <seealso cref="PendingTransfer"/>
[PublicAPI]
public sealed record PendingTransferCreation : Creation
{
	/// <inheritdoc cref="PendingTransfer.SourceAmount"/>
	[Required]
	public decimal? SourceAmount { get; init; }

	/// <inheritdoc cref="PendingTransfer.SourceAccountId"/>
	[Required]
	public Guid? SourceAccountId { get; init; }

	/// <inheritdoc cref="PendingTransfer.TargetCounterpartyId"/>
	[Required]
	public Guid? TargetCounterpartyId { get; init; }

	/// <inheritdoc cref="PendingTransfer.TransferId"/>
	public Guid? TransferId { get; init; }
}
