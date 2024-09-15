// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Models.Projects;

namespace Gnomeshade.Avalonia.Core.Projects;

/// <summary>Overview of a single <see cref="Project"/>.</summary>
public sealed class ProjectRow : PropertyChangedBase
{
	private decimal _total;

	/// <summary>Initializes a new instance of the <see cref="ProjectRow"/> class.</summary>
	/// <param name="project">The project this row will represent.</param>
	public ProjectRow(Project project)
	{
		Id = project.Id;
		Name = project.Name;
	}

	/// <summary>Gets the name of the project.</summary>
	public string Name { get; }

	/// <summary>Gets the sum of all purchases that are part of the project.</summary>
	public decimal Total
	{
		get => _total;
		internal set => SetAndNotify(ref _total, value);
	}

	internal Guid Id { get; }
}
