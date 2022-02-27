// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Tags;

/// <summary>A keyword that can be assigned to other data, for example, transaction items.</summary>
[PublicAPI]
public sealed record Tag
{
	/// <summary>The id of the tag.</summary>
	public Guid Id { get; init; }

	/// <summary>The point in time when the tag was created.</summary>
	public DateTimeOffset CreatedAt { get; init; }

	/// <summary>The id of the owner of the tag.</summary>
	public Guid OwnerId { get; init; }

	/// <summary>The id of the user that created this tag.</summary>
	public Guid CreatedByUserId { get; init; }

	/// <summary>The point in the when the tag was last modified.</summary>
	public DateTimeOffset ModifiedAt { get; init; }

	/// <summary>The id of the user that last modified this tag.</summary>
	public Guid ModifiedByUserId { get; init; }

	/// <summary>The name of the tag.</summary>
	public string Name { get; init; } = null!;

	/// <summary>The description of the tag.</summary>
	public string? Description { get; init; }
}
