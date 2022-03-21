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
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private ProductRow? _selectedProduct;
	private ProductCreationViewModel _product;
	private DataGridItemCollectionView<ProductRow> _products;

	private ProductViewModel(
		IGnomeshadeClient gnomeshadeClient,
		IEnumerable<ProductRow> productRows,
		ProductCreationViewModel productCreationViewModel)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_product = productCreationViewModel;
		Product.Upserted += OnProductUpserted;

		_products = new(productRows);

		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Gets the grid view of all products.</summary>
	public DataGridCollectionView DataGridView => Products;

	/// <summary>Gets a typed collection of all products.</summary>
	public DataGridItemCollectionView<ProductRow> Products
	{
		get => _products;
		private set => SetAndNotifyWithGuard(ref _products, value, nameof(Products), nameof(DataGridView));
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
			Product.Upserted -= OnProductUpserted;
			SetAndNotify(ref _product, value);
			Product.Upserted += OnProductUpserted;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="ProductViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <returns>A new instance of the <see cref="ProductViewModel"/> class.</returns>
	public static async Task<ProductViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
	{
		var productRows = await gnomeshadeClient.GetProductRowsAsync().ConfigureAwait(false);
		var productCreation = await ProductCreationViewModel.CreateAsync(gnomeshadeClient).ConfigureAwait(false);

		return new(gnomeshadeClient, productRows, productCreation);
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not nameof(SelectedProduct))
		{
			return;
		}

		Product = Task.Run(() => ProductCreationViewModel.CreateAsync(_gnomeshadeClient, SelectedProduct?.Id)).Result;
	}

	private void OnProductUpserted(object? sender, UpsertedEventArgs e)
	{
		var productRowsTask = Task.Run(() => _gnomeshadeClient.GetProductRowsAsync());
		var productCreationTask = Task.Run(() => ProductCreationViewModel.CreateAsync(_gnomeshadeClient));

		var sortDescriptions = DataGridView.SortDescriptions;
		Products = new(productRowsTask.GetAwaiter().GetResult());
		DataGridView.SortDescriptions.AddRange(sortDescriptions);

		Product = productCreationTask.GetAwaiter().GetResult();
	}
}
