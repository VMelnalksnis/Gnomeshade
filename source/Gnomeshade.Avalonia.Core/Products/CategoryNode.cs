// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Gnomeshade.WebApi.Models.Products;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>A hierarchical representation of a <see cref="Category"/>.</summary>
public sealed class CategoryNode
{
	private CategoryNode(ObservableCollection<CategoryNode> nodes, string name, Guid id)
	{
		Nodes = nodes;
		Name = name;
		Id = id;
	}

	/// <summary>Gets all categories in this category.</summary>
	public ObservableCollection<CategoryNode> Nodes { get; }

	/// <summary>Gets the name of the category.</summary>
	public string Name { get; }

	/// <summary>Gets the id of the category.</summary>
	public Guid Id { get; }

	/// <summary>Initializes a new instance of the <see cref="CategoryNode"/> class.</summary>
	/// <param name="category">The category for which to create the node.</param>
	/// <param name="categories">All other categories from which to create child nodes.</param>
	/// <returns>A new instance of the <see cref="CategoryNode"/> class.</returns>
	public static CategoryNode FromCategory(Category category, List<Category> categories)
	{
		var nodes = categories.Where(c => c.CategoryId == category.Id).Select(c => FromCategory(c, categories));
		return new(new(nodes), category.Name, category.Id);
	}

	/// <summary>Checks whether this node or any node in <see cref="Nodes"/> has the specified id.</summary>
	/// <param name="id">The id for which to check.</param>
	/// <returns><see langword="true"/> if this node or any node in <see cref="Nodes"/> has the <paramref name="id"/>; otherwise <see langword="false"/>.</returns>
	public bool Contains(Guid id) => Id == id || Nodes.Any(node => node.Contains(id));

	/// <summary>Finds the node with the specified id.</summary>
	/// <param name="id">The id of the node to find.</param>
	/// <returns>The node with the specified id if it exists; otherwise <c>null</c>.</returns>
	public CategoryNode? Find(Guid id)
	{
		if (Id == id)
		{
			return this;
		}

		if (Nodes.FirstOrDefault(node => node.Id == id) is { } foundNode)
		{
			return foundNode;
		}

		return Nodes.SelectMany(node => node.Nodes).Select(node => node.Find(id)).FirstOrDefault();
	}
}
