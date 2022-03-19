// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

/// <summary>Overview and editing of all products.</summary>
public sealed class ProductViewModel : ViewModelBase
{
	private readonly IProductClient _productClient;

	private ProductRow? _selectedProduct;
	private ProductCreationViewModel _product;
	private DataGridItemCollectionView<ProductRow> _products;

	private ProductViewModel(
		IProductClient productClient,
		IEnumerable<ProductRow> productRows,
		ProductCreationViewModel productCreationViewModel)
	{
		_productClient = productClient;
		_product = productCreationViewModel;
		Product.ProductCreated += OnProductCreated;

		_products = new(productRows);

		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Gets the grid view of all products.</summary>
	public DataGridCollectionView DataGridView => Products;

	/// <summary>Gets a typed collection of all products.</summary>
	public DataGridItemCollectionView<ProductRow> Products
	{
		get => _products;
		private set => SetAndNotify(ref _products, value);
	}

	/// <summary>Gets or sets the selected product from <see cref="DataGridView"/>.</summary>
	public ProductRow? SelectedProduct
	{
		get => _selectedProduct;
		set => SetAndNotify(ref _selectedProduct, value);
	}

	/// <summary>Gets the current product creation view model.</summary>
	public ProductCreationViewModel Product
	{
		get => _product;
		private set
		{
			Product.ProductCreated -= OnProductCreated;
			SetAndNotify(ref _product, value);
			Product.ProductCreated += OnProductCreated;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="ProductViewModel"/> class.</summary>
	/// <param name="productClient">Gnomeshade API client.</param>
	/// <returns>A new instance of the <see cref="ProductViewModel"/> class.</returns>
	public static async Task<ProductViewModel> CreateAsync(IProductClient productClient)
	{
		var productRows = await productClient.GetProductRowsAsync().ConfigureAwait(false);
		var productCreation = await ProductCreationViewModel.CreateAsync(productClient).ConfigureAwait(false);

		return new(productClient, productRows, productCreation);
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not nameof(SelectedProduct))
		{
			return;
		}

		Product = Task.Run(() => ProductCreationViewModel.CreateAsync(_productClient, SelectedProduct?.Id)).Result;
	}

	private void OnProductCreated(object? sender, ProductCreatedEventArgs e)
	{
		var productRowsTask = Task.Run(() => _productClient.GetProductRowsAsync());
		var productCreationTask = Task.Run(() => ProductCreationViewModel.CreateAsync(_productClient));

		var sortDescriptions = DataGridView.SortDescriptions;
		Products = new(productRowsTask.GetAwaiter().GetResult());
		DataGridView.SortDescriptions.AddRange(sortDescriptions);

		Product = productCreationTask.GetAwaiter().GetResult();
	}
}
