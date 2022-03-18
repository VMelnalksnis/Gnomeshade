// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Transactions;

/// <summary>Information needed in order to create a transaction.</summary>
[PublicAPI]
public sealed record TransactionCreationModel
{
	/// <inheritdoc cref="Transaction.BookedAt"/>
	[RequiredIfNull(nameof(ValuedAt))]
	public DateTimeOffset? BookedAt { get; init; }

	/// <inheritdoc cref="Transaction.ValuedAt"/>
	[RequiredIfNull(nameof(BookedAt))]
	public DateTimeOffset? ValuedAt { get; init; }

	/// <inheritdoc cref="Transaction.Description"/>
	public string? Description { get; init; }

	/// <inheritdoc cref="Transaction.ReconciledAt"/>
	public DateTimeOffset? ReconciledAt { get; init; }

	/// <summary>SHA512 hash of the imported data.</summary>
	[MinLength(64)]
	[MaxLength(64)]
	public byte[]? ImportHash { get; init; }

	/// <inheritdoc cref="Transaction.Items"/>
	public List<TransactionItemCreationModel>? Items { get; init; }
}
