// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.Avalonia.Core.DesignTime;
using Gnomeshade.Interfaces.Avalonia.Core.Products;

using NodaTime;

using static Gnomeshade.Interfaces.Avalonia.Core.Products.ProductViewModel;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests.Products;

[TestOf(typeof(ProductViewModel))]
public sealed class ProductViewModelTests
{
	[Test]
	public async Task Product_SaveAsync_ShouldUpdateDataGridView()
	{
		var viewModel = await CreateAsync(new DesignTimeGnomeshadeClient(), DateTimeZoneProviders.Tzdb);
		viewModel.Rows.Should().HaveCount(2);

		var newProductName = Guid.NewGuid().ToString("N");
		viewModel.Details.Name = newProductName;
		await viewModel.Details.SaveAsync();

		viewModel.Rows.Should().HaveCount(3).And.ContainSingle(product => product.Name == newProductName);
	}

	[Test]
	public async Task SelectedProduct_ShouldUpdateProductCreationViewModel()
	{
		var viewModel = await CreateAsync(new DesignTimeGnomeshadeClient(), DateTimeZoneProviders.Tzdb);
		viewModel.Details.Name.Should().BeNullOrWhiteSpace();

		var productToSelect = viewModel.Rows.First();
		viewModel.Selected = productToSelect;

		viewModel.Details.Name.Should().Be(productToSelect.Name);
	}
}
