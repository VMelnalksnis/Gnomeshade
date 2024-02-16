// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using Gnomeshade.WebApi.Models.Products;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Values for filtering categories.</summary>
/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
public sealed partial class CategoryFilter(IActivityService activityService) : FilterBase<CategoryRow>(activityService)
{
	/// <summary>Gets or sets a collection of all categories.</summary>
	[Notify]
	private List<Category> _categories = [];

	/// <summary>Gets or sets a collection of all categories in a hierarchical structure.</summary>
	[Notify]
	private List<CategoryNode> _nodes = [];

	/// <summary>Gets or sets the text by which to filter categories.</summary>
	[Notify]
	private string? _filterText;

	/// <summary>Gets or sets the parent category by which to filter categories.</summary>
	[Notify]
	private Category? _selectedCategory;

	/// <summary>Gets a delegate for formatting a category in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CategorySelector => AutoCompleteSelectors.Category;

	/// <inheritdoc />
	protected override bool FilterRow(CategoryRow row)
	{
		if (FilterText is null && SelectedCategory is null)
		{
			return true;
		}

		return MatchesFilterText(row) && MatchesSelectedCategory(row);
	}

	private bool MatchesFilterText(CategoryRow row)
	{
		return FilterText is null || row.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
	}

	private bool MatchesSelectedCategory(CategoryRow row)
	{
		if (SelectedCategory is null)
		{
			return true;
		}

		var node = Nodes.Select(node => node.Find(SelectedCategory.Id)).First(node => node is not null)!;
		return node.Contains(row.Id);
	}
}
