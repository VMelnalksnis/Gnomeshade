// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models;

/// <summary>A link to an external resource.</summary>
/// <seealso cref="LinkCreation"/>
[PublicAPI]
public sealed record Link
{
	/// <summary>The id of the link.</summary>
	public Guid Id { get; set; }

	/// <summary>The point in time when the link was created.</summary>
	public Instant CreatedAt { get; set; }

	/// <summary>The id of the owner of the link.</summary>
	public Guid OwnerId { get; set; }

	/// <summary>The id of the user that created this link.</summary>
	public Guid CreatedByUserId { get; set; }

	/// <summary>The point in the when the link was last modified.</summary>
	public Instant ModifiedAt { get; set; }

	/// <summary>The id of the user that last modified this link.</summary>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>The unescaped canonical representation of the uniform resource identifier of the linked data.</summary>
	public string Uri { get; set; } = null!;
}
