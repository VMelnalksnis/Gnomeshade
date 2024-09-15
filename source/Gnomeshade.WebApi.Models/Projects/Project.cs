// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Projects;

/// <summary>A grouping of purchases.</summary>
/// <seealso cref="Purchase"/>
public sealed record Project
{
	/// <summary>The id of the project.</summary>
	public Guid Id { get; set; }

	/// <summary>The point in time when the project was created.</summary>
	public Instant CreatedAt { get; set; }

	/// <summary>The id of the owner of the project.</summary>
	public Guid OwnerId { get; set; }

	/// <summary>The id of the user that created this project.</summary>
	public Guid CreatedByUserId { get; set; }

	/// <summary>The point in the when the project was last modified.</summary>
	public Instant ModifiedAt { get; set; }

	/// <summary>The id of the user that last modified this project.</summary>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>The name of the project.</summary>
	public string Name { get; set; } = null!;

	/// <summary>The id of the parent project.</summary>
	/// <seealso cref="Project"/>
	public Guid? ParentProjectId { get; set; }
}
