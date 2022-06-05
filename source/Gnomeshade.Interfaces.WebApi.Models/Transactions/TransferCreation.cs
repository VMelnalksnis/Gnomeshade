// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Transactions;

/// <summary>Information needed to create a transfer.</summary>
/// <seealso cref="Transfer"/>
[PublicAPI]
public sealed record TransferCreation : Creation
{
	/// <inheritdoc cref="Transfer.SourceAmount"/>
	[Required]
	public decimal? SourceAmount { get; init; }

	/// <inheritdoc cref="Transfer.SourceAccountId"/>
	[Required]
	public Guid? SourceAccountId { get; init; }

	/// <inheritdoc cref="Transfer.TargetAmount"/>
	[Required]
	public decimal? TargetAmount { get; init; }

	/// <inheritdoc cref="Transfer.TargetAccountId"/>
	[Required]
	public Guid? TargetAccountId { get; init; }

	/// <inheritdoc cref="Transfer.BankReference"/>
	public string? BankReference { get; init; }

	/// <inheritdoc cref="Transfer.ExternalReference"/>
	public string? ExternalReference { get; init; }

	/// <inheritdoc cref="Transfer.InternalReference"/>
	public string? InternalReference { get; init; }
}
