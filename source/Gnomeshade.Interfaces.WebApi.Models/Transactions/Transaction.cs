// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Transactions;

[PublicAPI]
public sealed record Transaction
{
	public Guid Id { get; init; }

	public Guid UserId { get; init; }

	public DateTimeOffset CreatedAt { get; init; }

	public Guid CreatedByUserId { get; init; }

	public DateTimeOffset ModifiedAt { get; init; }

	public DateTimeOffset Date { get; init; }

	public string? Description { get; init; }

	public DateTimeOffset? ImportedAt { get; init; }

	public bool Imported => ImportedAt.HasValue;

	public DateTimeOffset? ValidatedAt { get; init; }

	public bool Validated => ValidatedAt.HasValue;

	public List<TransactionItem> Items { get; init; }
}
