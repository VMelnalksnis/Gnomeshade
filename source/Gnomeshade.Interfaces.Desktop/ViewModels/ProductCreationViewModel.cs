// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Interfaces.Desktop.ViewModels.Events;
using Gnomeshade.Interfaces.Desktop.Views;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Interfaces.Desktop.ViewModels;

/// <summary>
/// Form for creating a single new product.
/// </summary>
public sealed class ProductCreationViewModel : ViewModelBase<ProductCreationView>
{
	private readonly IProductClient _productClient;
	private readonly Product? _exisingProduct;

	private string? _name;
	private Unit? _selectedUnit;
	private string? _description;

	private ProductCreationViewModel(IProductClient productClient, List<Unit> units)
	{
		_productClient = productClient;
		Units = units;

		UnitSelector = (_, item) => ((Unit)item).Name;
	}

	private ProductCreationViewModel(IProductClient productClient, List<Unit> units, Product product)
		: this(productClient, units)
	{
		_exisingProduct = product;

		Name = _exisingProduct.Name;
		Description = _exisingProduct.Description;
		SelectedUnit = _exisingProduct.UnitId is null
			? null
			: Units.Single(unit => unit.Id == _exisingProduct.UnitId.Value);
	}

	/// <summary>
	/// Raised when a new product has been successfully created.
	/// </summary>
	public event EventHandler<ProductCreatedEventArgs>? ProductCreated;

	/// <summary>
	/// Gets or sets the name of the product.
	/// </summary>
	public string? Name
	{
		get => _name;
		set => SetAndNotifyWithGuard(ref _name, value, nameof(Name), nameof(CanSave));
	}

	/// <summary>
	/// Gets or sets the description of the product.
	/// </summary>
	public string? Description
	{
		get => _description;
		set => SetAndNotify(ref _description, value, nameof(Description));
	}

	/// <summary>
	/// Gets or sets the unit in which an amount of this product is measured in.
	/// </summary>
	public Unit? SelectedUnit
	{
		get => _selectedUnit;
		set => SetAndNotify(ref _selectedUnit, value, nameof(SelectedUnit));
	}

	/// <summary>
	/// Gets a collection of all available units.
	/// </summary>
	public List<Unit> Units { get; }

	/// <summary>
	/// Gets a delegate for formatting a unit in an <see cref="AutoCompleteBox"/>.
	/// </summary>
	public AutoCompleteSelector<object> UnitSelector { get; }

	/// <summary>
	/// Gets a value indicating whether a product can be created from currently provided values.
	/// </summary>
	public bool CanSave => !string.IsNullOrWhiteSpace(Name);

	/// <summary>
	/// Initializes a new instance of the <see cref="ProductCreationViewModel"/> class.
	/// </summary>
	/// <param name="productClient">Gnomeshade API client.</param>
	/// <param name="productId">The id of the product to edit.</param>
	/// <returns>A new instance of the <see cref="ProductCreationViewModel"/> class.</returns>
	public static async Task<ProductCreationViewModel> CreateAsync(
		IProductClient productClient,
		Guid? productId = null)
	{
		var units = await productClient.GetUnitsAsync();
		if (productId is null)
		{
			return new(productClient, units);
		}

		var product = await productClient.GetProductAsync(productId.Value);
		return new(productClient, units, product);
	}

	/// <summary>
	/// Creates a new product from the provided values.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SaveAsync()
	{
		var creationModel = new ProductCreationModel
		{
			Name = Name,
			Description = Description,
			UnitId = SelectedUnit?.Id,
		};

		var id = _exisingProduct?.Id ?? Guid.NewGuid();
		await _productClient.PutProductAsync(id, creationModel).ConfigureAwait(false);
		OnProductCreated(id);
	}

	private void OnProductCreated(Guid productId)
	{
		ProductCreated?.Invoke(this, new(productId));
	}
}
