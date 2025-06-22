// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>Information needed to create a transfer.</summary>
/// <seealso cref="Transfer"/>
[PublicAPI]
public sealed record TransferCreation : TransferCreationBase
{
	/// <inheritdoc cref="Transfer.SourceAccountId"/>
	[Required]
	public override Guid? SourceAccountId { get; set; }

	/// <inheritdoc cref="Transfer.TargetAccountId"/>
	[Required]
	public override Guid? TargetAccountId { get; set; }

	/// <inheritdoc cref="Transfer.BankReference"/>
	public string? BankReference { get; set; }

	/// <inheritdoc cref="Transfer.ExternalReference"/>
	public string? ExternalReference { get; set; }

	/// <inheritdoc cref="Transfer.InternalReference"/>
	public string? InternalReference { get; set; }

	/// <inheritdoc cref="TransferBase.BookedAt"/>
	[RequiredIfNull(nameof(ValuedAt))]
	public override Instant? BookedAt { get; set; }

	/// <inheritdoc cref="Transfer.ValuedAt"/>
	[RequiredIfNull(nameof(BookedAt))]
	public Instant? ValuedAt { get; set; }
}
