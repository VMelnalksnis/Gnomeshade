// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.Interfaces.WebApi.Models.Transactions;

/// <summary>Information needed in order to create a transaction.</summary>
[PublicAPI]
public sealed record TransactionCreation : Creation
{
	/// <inheritdoc cref="Transaction.BookedAt"/>
	[RequiredIfNull(nameof(ValuedAt))]
	public Instant? BookedAt { get; init; }

	/// <inheritdoc cref="Transaction.ValuedAt"/>
	[RequiredIfNull(nameof(BookedAt))]
	public Instant? ValuedAt { get; init; }

	/// <inheritdoc cref="Transaction.Description"/>
	public string? Description { get; init; }

	/// <inheritdoc cref="Transaction.ReconciledAt"/>
	public Instant? ReconciledAt { get; init; }

	/// <summary>SHA512 hash of the imported data.</summary>
	[MinLength(64)]
	[MaxLength(64)]
	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = $"Implements {nameof(ICollection)}")]
	public byte[]? ImportHash { get; init; }

	/// <inheritdoc cref="Transaction.ImportedAt"/>
	public Instant? ImportedAt { get; init; }
}
