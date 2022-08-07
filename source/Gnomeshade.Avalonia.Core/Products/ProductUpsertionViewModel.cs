// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core.Transactions.Purchases;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Products;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Form for creating a single new product.</summary>
public sealed class ProductUpsertionViewModel : UpsertionViewModel
{
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private Guid? _id;
	private string? _name;
	private Unit? _selectedUnit;
	private string? _sku;
	private string? _description;
	private Category? _selectedCategory;
	private List<Unit> _units;
	private List<Category> _categories;
	private List<PurchaseOverview> _purchases;

	/// <summary>Initializes a new instance of the <see cref="ProductUpsertionViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="id">The id of the product to edit.</param>
	public ProductUpsertionViewModel(
		IGnomeshadeClient gnomeshadeClient,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid? id)
		: base(gnomeshadeClient)
	{
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_id = id;

		_units = new();
		_categories = new();
		_purchases = new();
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
	public List<Unit> Units
	{
		get => _units;
		private set => SetAndNotify(ref _units, value);
	}

	/// <summary>Gets a delegate for formatting a unit in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> UnitSelector => AutoCompleteSelectors.Unit;

	/// <summary>Gets a collection of all available categories.</summary>
	public List<Category> Categories
	{
		get => _categories;
		private set => SetAndNotify(ref _categories, value);
	}

	/// <summary>Gets a delegate for formatting a category in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CategorySelector => AutoCompleteSelectors.Category;

	/// <summary>Gets all purchases of this product.</summary>
	public List<PurchaseOverview> Purchases
	{
		get => _purchases;
		private set => SetAndNotify(ref _purchases, value);
	}

	/// <inheritdoc />
	public override bool CanSave => !string.IsNullOrWhiteSpace(Name);

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var units = await GnomeshadeClient.GetUnitsAsync().ConfigureAwait(false);
		var categories = await GnomeshadeClient.GetCategoriesAsync().ConfigureAwait(false);

		Units = units;
		Categories = categories;
		if (_id is not { } productId)
		{
			Purchases = new();
			return;
		}

		var product = await GnomeshadeClient.GetProductAsync(productId);

		Name = product.Name;
		Sku = product.Sku;
		Description = product.Description;
		SelectedUnit = product.UnitId is null
			? null
			: Units.Single(unit => unit.Id == product.UnitId.Value);
		SelectedCategory = product.CategoryId is null
			? null
			: Categories.SingleOrDefault(category => category.Id == product.CategoryId.Value);

		var purchases = await GnomeshadeClient.GetProductPurchasesAsync(productId).ConfigureAwait(false);
		var currencies = await GnomeshadeClient.GetCurrenciesAsync().ConfigureAwait(false);
		var products = new[] { product };
		var overviews = purchases
			.Select(purchase => purchase.ToOverview(currencies, products, units, _dateTimeZoneProvider))
			.ToList();

		Purchases = overviews;
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var creationModel = new ProductCreation
		{
			Name = Name,
			Sku = Sku,
			Description = Description,
			UnitId = SelectedUnit?.Id,
			CategoryId = SelectedCategory?.Id,
		};

		_id ??= Guid.NewGuid();
		await GnomeshadeClient.PutProductAsync(_id.Value, creationModel).ConfigureAwait(false);
		return _id.Value;
	}
}
