// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Products;

/// <summary>Overview and editing of all products.</summary>
public sealed class ProductViewModel : OverviewViewModel<ProductRow, ProductCreationViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private ProductCreationViewModel _details;

	private ProductViewModel(IGnomeshadeClient gnomeshadeClient, ProductCreationViewModel productCreationViewModel)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_details = productCreationViewModel;

		Details.Upserted += OnProductUpserted;
		PropertyChanged += OnPropertyChanged;
	}

	/// <inheritdoc />
	public override ProductCreationViewModel Details
	{
		get => _details;
		set
		{
			Details.Upserted -= OnProductUpserted;
			SetAndNotify(ref _details, value);
			Details.Upserted += OnProductUpserted;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="ProductViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <returns>A new instance of the <see cref="ProductViewModel"/> class.</returns>
	public static async Task<ProductViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
	{
		var creationViewModel = await ProductCreationViewModel.CreateAsync(gnomeshadeClient).ConfigureAwait(false);
		var viewModel = new ProductViewModel(gnomeshadeClient, creationViewModel);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <inheritdoc />
	public override async Task RefreshAsync()
	{
		var productRows = (await _gnomeshadeClient.GetProductRowsAsync().ConfigureAwait(false)).ToList();
		var creationViewModel = await ProductCreationViewModel.CreateAsync(_gnomeshadeClient).ConfigureAwait(false);

		var sortDescriptions = DataGridView.SortDescriptions;
		Rows = new(productRows);
		DataGridView.SortDescriptions.AddRange(sortDescriptions);

		Details = creationViewModel;
	}

	/// <inheritdoc />
	protected override Task DeleteAsync(ProductRow row) => throw new NotImplementedException();

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not nameof(Selected))
		{
			return;
		}

		Details = Task.Run(() => ProductCreationViewModel.CreateAsync(_gnomeshadeClient, Selected?.Id)).Result;
	}

	private void OnProductUpserted(object? sender, UpsertedEventArgs e)
	{
		Task.Run(RefreshAsync).GetAwaiter().GetResult();
	}
}
