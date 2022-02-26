// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

namespace Gnomeshade.Data.Entities;

/// <summary>Tags a <see cref="TagEntity"/> with a <see cref="TagEntity"/>.</summary>
public sealed record TagTagEntity : ITaggingEntity<TagEntity>
{
	/// <inheritdoc />
	public DateTimeOffset CreatedAt { get; init; }

	/// <inheritdoc />
	public Guid TagId { get; init; }

	/// <inheritdoc />
	public Guid TaggedItemId { get; init; }
}
