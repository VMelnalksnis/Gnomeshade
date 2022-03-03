// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tags;

/// <summary>Overview of a single tag.</summary>
public sealed class TagRow : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="TagRow"/> class.</summary>
	/// <param name="id">The id of the tag.</param>
	/// <param name="name">The name of the tag.</param>
	/// <param name="description">The description of the tag.</param>
	public TagRow(Guid id, string name, string? description)
	{
		Id = id;
		Name = name;
		Description = description;
	}

	/// <summary>Gets the id of the tag.</summary>
	public Guid Id { get; }

	/// <summary>Gets the name of the tag.</summary>
	public string Name { get; }

	/// <summary>Gets the description of the tag.</summary>
	public string? Description { get; }
}
