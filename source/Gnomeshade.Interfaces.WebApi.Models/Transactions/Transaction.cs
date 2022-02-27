// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Transactions;

/// <summary>A financial transaction during which funds can be transferred between multiple accounts.</summary>
[PublicAPI]
public sealed record Transaction
{
	/// <summary>The id of the transaction.</summary>
	public Guid Id { get; init; }

	/// <summary>The id of the owner of the transaction.</summary>
	public Guid OwnerId { get; init; }

	/// <summary>The point in time when the transaction was created.</summary>
	public DateTimeOffset CreatedAt { get; init; }

	/// <summary>The id of the user that created this transaction.</summary>
	public Guid CreatedByUserId { get; init; }

	/// <summary>The point in the when the transaction was last modified.</summary>
	public DateTimeOffset ModifiedAt { get; init; }

	/// <summary>The id of the user that last modified this transaction.</summary>
	public Guid ModifiedByUserId { get; init; }

	/// <summary>The point in time when this transaction was completed.</summary>
	public DateTimeOffset Date { get; init; }

	/// <summary>The description of the transaction.</summary>
	public string? Description { get; init; }

	/// <summary>The point in time when this transaction was imported.</summary>
	public DateTimeOffset? ImportedAt { get; init; }

	/// <summary>Whether or not this transaction was imported.</summary>
	public bool Imported => ImportedAt.HasValue;

	/// <summary>The point in time when this transaction was validated.</summary>
	public DateTimeOffset? ValidatedAt { get; init; }

	/// <summary>Whether or not this transaction was validated.</summary>
	public bool Validated => ValidatedAt.HasValue;

	/// <summary>All items that are a part of this transaction.</summary>
	public List<TransactionItem> Items { get; init; } = null!;
}
