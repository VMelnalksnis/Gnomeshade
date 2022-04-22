// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

/// <summary>Form for creating a single new product.</summary>
public sealed class ProductCreationViewModel : UpsertionViewModel
{
	private readonly Product? _exisingProduct;

	private string? _name;
	private Unit? _selectedUnit;
	private string? _sku;
	private string? _description;
	private Category? _selectedCategory;

	private ProductCreationViewModel(IGnomeshadeClient gnomeshadeClient, List<Unit> units, List<Category> categories)
		: base(gnomeshadeClient)
	{
		Units = units;
		Categories = categories;
	}

	private ProductCreationViewModel(IGnomeshadeClient gnomeshadeClient, List<Unit> units, List<Category> categories, Product product)
		: this(gnomeshadeClient, units, categories)
	{
		_exisingProduct = product;

		Name = _exisingProduct.Name;
		Sku = _exisingProduct.Sku;
		Description = _exisingProduct.Description;
		SelectedUnit = _exisingProduct.UnitId is null
			? null
			: Units.Single(unit => unit.Id == _exisingProduct.UnitId.Value);
	}

	/// <summary>Gets or sets the name of the product.</summary>
	public string? Name
	{
		get => _name;
		set => SetAndNotifyWithGuard(ref _name, value, nameof(Name), nameof(CanSave));
	}

	/// <summary>Gets or sets the SKU of the product.</summary>
	public string? Sku
	{
		get => _sku;
		set => SetAndNotify(ref _sku, value);
	}

	/// <summary>Gets or sets the description of the product.</summary>
	public string? Description
	{
		get => _description;
		set => SetAndNotify(ref _description, value);
	}

	/// <summary>Gets or sets the unit in which an amount of this product is measured in.</summary>
	public Unit? SelectedUnit
	{
		get => _selectedUnit;
		set => SetAndNotify(ref _selectedUnit, value);
	}

	/// <summary>Gets or sets the category of this product.</summary>
	public Category? SelectedCategory
	{
		get => _selectedCategory;
		set => SetAndNotify(ref _selectedCategory, value);
	}

	/// <summary>Gets a collection of all available units.</summary>
	public List<Unit> Units { get; }

	/// <summary>Gets a delegate for formatting a unit in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> UnitSelector => AutoCompleteSelectors.Unit;

	/// <summary>Gets a collection of all available categories.</summary>
	public List<Category> Categories { get; }

	/// <summary>Gets a delegate for formatting a category in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CategorySelector => AutoCompleteSelectors.Category;

	/// <inheritdoc />
	public override bool CanSave => !string.IsNullOrWhiteSpace(Name);

	/// <summary>Initializes a new instance of the <see cref="ProductCreationViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="productId">The id of the product to edit.</param>
	/// <returns>A new instance of the <see cref="ProductCreationViewModel"/> class.</returns>
	public static async Task<ProductCreationViewModel> CreateAsync(
		IGnomeshadeClient gnomeshadeClient,
		Guid? productId = null)
	{
		var units = await gnomeshadeClient.GetUnitsAsync().ConfigureAwait(false);
		var categories = await gnomeshadeClient.GetCategoriesAsync().ConfigureAwait(false);
		if (productId is null)
		{
			return new(gnomeshadeClient, units, categories);
		}

		var product = await gnomeshadeClient.GetProductAsync(productId.Value);
		return new(gnomeshadeClient, units, categories, product);
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var creationModel = new ProductCreationModel
		{
			Name = Name,
			Sku = Sku,
			Description = Description,
			UnitId = SelectedUnit?.Id,
		};

		var id = _exisingProduct?.Id ?? Guid.NewGuid();
		await GnomeshadeClient.PutProductAsync(id, creationModel).ConfigureAwait(false);
		return id;
	}
}
