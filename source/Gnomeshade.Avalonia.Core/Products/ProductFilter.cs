// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using Avalonia.Controls;

using Gnomeshade.WebApi.Models.Products;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Values for filtering products.</summary>
/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
public sealed partial class ProductFilter(IActivityService activityService) : FilterBase<ProductRow>(activityService)
{
	/// <summary>Gets or sets a collection of all available units.</summary>
	[Notify]
	private List<Unit> _units = [];

	/// <summary>Gets or sets a collection of all available categories.</summary>
	[Notify]
	private List<Category> _categories = [];

	/// <summary>Gets or sets the text by which to filter product names.</summary>
	[Notify]
	private string? _filterText;

	/// <summary>Gets or sets the selected unit from <see cref="Units"/>.</summary>
	[Notify]
	private Unit? _selectedUnit;

	/// <summary>Gets or sets the selected category from <see cref="Categories"/>.</summary>
	[Notify]
	private Category? _selectedCategory;

	/// <summary>Gets a delegate for formatting a unit in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> UnitSelector => AutoCompleteSelectors.Unit;

	/// <summary>Gets a delegate for formatting a category in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CategorySelector => AutoCompleteSelectors.Category;

	/// <inheritdoc />
	protected override bool FilterRow(ProductRow row)
	{
		if (FilterText is null && SelectedUnit is null && SelectedCategory is null)
		{
			return true;
		}

		var nameMatches = FilterText is not { } text || row.Name.Contains(text, StringComparison.OrdinalIgnoreCase);
		var unitMatches = SelectedUnit is null || row.UnitId == SelectedUnit.Id;
		var categoryMatches = SelectedCategory is null || row.CategoryId == SelectedCategory.Id;

		return nameMatches && unitMatches && categoryMatches;
	}
}
