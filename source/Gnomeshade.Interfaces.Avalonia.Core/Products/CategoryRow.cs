// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

/// <summary>Overview of a single category.</summary>
public sealed class CategoryRow : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="CategoryRow"/> class.</summary>
	/// <param name="id">The id of the category.</param>
	/// <param name="name">The name of the category.</param>
	/// <param name="description">The description of the category.</param>
	/// <param name="categoryName">The name of the category to which the category belongs to.</param>
	public CategoryRow(Guid id, string name, string? description, string? categoryName)
	{
		Id = id;
		Name = name;
		Description = description;
		CategoryName = categoryName;
	}

	/// <summary>Gets the id of the category.</summary>
	public Guid Id { get; }

	/// <summary>Gets the name of the category.</summary>
	public string Name { get; }

	/// <summary>Gets the description of the category.</summary>
	public string? Description { get; }

	/// <summary>Gets the name of the category to which the category belongs to.</summary>
	public string? CategoryName { get; }
}
