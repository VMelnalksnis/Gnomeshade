// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using Avalonia.Controls;

using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Values for filtering products.</summary>
public sealed class ProductFilter : FilterBase<ProductRow>
{
	private List<Unit> _units = new();
	private Unit? _selectedUnit;
	private List<Category> _categories = new();
	private Category? _selectedCategory;

	/// <summary>Gets a delegate for formatting a unit in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> UnitSelector => AutoCompleteSelectors.Unit;

	/// <summary>Gets a delegate for formatting a category in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CategorySelector => AutoCompleteSelectors.Category;

	/// <summary>Gets or sets a collection of all available units.</summary>
	public List<Unit> Units
	{
		get => _units;
		set => SetAndNotify(ref _units, value);
	}

	/// <summary>Gets or sets the selected unit from <see cref="Units"/>.</summary>
	public Unit? SelectedUnit
	{
		get => _selectedUnit;
		set => SetAndNotify(ref _selectedUnit, value);
	}

	/// <summary>Gets or sets a collection of all available categories.</summary>
	public List<Category> Categories
	{
		get => _categories;
		set => SetAndNotify(ref _categories, value);
	}

	/// <summary>Gets or sets the selected category from <see cref="Categories"/>.</summary>
	public Category? SelectedCategory
	{
		get => _selectedCategory;
		set => SetAndNotify(ref _selectedCategory, value);
	}

	/// <inheritdoc />
	protected override bool FilterRow(ProductRow row)
	{
		if (SelectedUnit is null && SelectedCategory is null)
		{
			return true;
		}

		var unitMatches = SelectedUnit is null || row.UnitId == SelectedUnit.Id;
		var categoryMatches = SelectedCategory is null || row.CategoryId == SelectedCategory.Id;

		return unitMatches && categoryMatches;
	}
}
