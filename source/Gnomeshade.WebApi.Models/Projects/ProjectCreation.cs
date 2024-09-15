// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Gnomeshade.WebApi.Models.Projects;

/// <summary>The information needed to create or update a project.</summary>
public sealed record ProjectCreation : Creation
{
	/// <inheritdoc cref="Project.Name"/>
	[Required]
	public string Name { get; set; } = null!;

	/// <inheritdoc cref="Project.ParentProjectId"/>
	public Guid? ParentProjectId { get; set; }
}
