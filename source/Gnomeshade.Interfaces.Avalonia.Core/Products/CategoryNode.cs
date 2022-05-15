// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

/// <summary>A hierarchical representation of a <see cref="Category"/>.</summary>
public sealed class CategoryNode
{
	private CategoryNode(ObservableCollection<CategoryNode> nodes, string name)
	{
		Nodes = nodes;
		Name = name;
	}

	/// <summary>Gets all categories in this category.</summary>
	public ObservableCollection<CategoryNode> Nodes { get; }

	/// <summary>Gets the name of the category.</summary>
	public string Name { get; }

	/// <summary>Initializes a new instance of the <see cref="CategoryNode"/> class.</summary>
	/// <param name="category">The category for which to create the node.</param>
	/// <param name="categories">All other categories from which to create child nodes.</param>
	/// <returns>A new instance of the <see cref="CategoryNode"/> class.</returns>
	public static CategoryNode FromCategory(Category category, List<Category> categories)
	{
		var nodes = categories.Where(c => c.CategoryId == category.Id).Select(c => CategoryNode.FromCategory(c, categories));
		return new(new(nodes), category.Name);
	}
}
