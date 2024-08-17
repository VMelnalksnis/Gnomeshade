// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core.Transactions.Purchases;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Products;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Form for creating a single new product.</summary>
public sealed partial class ProductUpsertionViewModel : UpsertionViewModel
{
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	/// <summary>Gets or sets the name of the product.</summary>
	[Notify]
	private string? _name;

	/// <summary>Gets or sets the unit in which an amount of this product is measured in.</summary>
	[Notify]
	private Unit? _selectedUnit;

	/// <summary>Gets or sets the SKU of the product.</summary>
	[Notify]
	private string? _sku;

	/// <summary>Gets or sets the description of the product.</summary>
	[Notify]
	private string? _description;

	/// <summary>Gets or sets the category of this product.</summary>
	[Notify]
	private Category? _selectedCategory;

	/// <summary>Gets a collection of all available units.</summary>
	[Notify(Setter.Private)]
	private List<Unit> _units = [];

	/// <summary>Gets a collection of all available categories.</summary>
	[Notify(Setter.Private)]
	private List<Category> _categories = [];

	/// <summary>Gets all purchases of this product.</summary>
	[Notify(Setter.Private)]
	private List<PurchaseOverview> _purchases = [];

	/// <summary>Initializes a new instance of the <see cref="ProductUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="id">The id of the product to edit.</param>
	public ProductUpsertionViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_dateTimeZoneProvider = dateTimeZoneProvider;
		Id = id;
	}

	/// <summary>Gets a delegate for formatting a unit in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> UnitSelector => AutoCompleteSelectors.Unit;

	/// <summary>Gets a delegate for formatting a category in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CategorySelector => AutoCompleteSelectors.Category;

	/// <inheritdoc />
	public override bool CanSave => !string.IsNullOrWhiteSpace(Name);

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var units = await GnomeshadeClient.GetUnitsAsync();
		var categories = await GnomeshadeClient.GetCategoriesAsync();

		Units = units;
		Categories = categories;
		if (Id is not { } productId)
		{
			Purchases = [];
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

		var purchases = await GnomeshadeClient.GetProductPurchasesAsync(productId);
		var currencies = await GnomeshadeClient.GetCurrenciesAsync();
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

		var id = Id ?? Guid.NewGuid();
		await GnomeshadeClient.PutProductAsync(id, creationModel);
		return id;
	}
}
