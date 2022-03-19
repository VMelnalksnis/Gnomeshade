// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.Avalonia.Core.DesignTime;
using Gnomeshade.Interfaces.Avalonia.Core.Products;

using NUnit.Framework;

using static Gnomeshade.Interfaces.Avalonia.Core.Products.ProductViewModel;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests.Products;

[TestOf(typeof(ProductViewModel))]
public class ProductViewModelTests
{
	[Test]
	public async Task Product_SaveAsync_ShouldUpdateDataGridView()
	{
		var viewModel = await CreateAsync(new DesignTimeGnomeshadeClient());
		viewModel.Products.Should().HaveCount(2);

		var newProductName = Guid.NewGuid().ToString("N");
		viewModel.Product.Name = newProductName;
		await viewModel.Product.SaveAsync();

		viewModel.Products.Should().HaveCount(3).And.ContainSingle(product => product.Name == newProductName);
	}

	[Test]
	public async Task SelectedProduct_ShouldUpdateProductCreationViewModel()
	{
		var viewModel = await CreateAsync(new DesignTimeGnomeshadeClient());
		viewModel.Product.Name.Should().BeNullOrWhiteSpace();

		var productToSelect = viewModel.Products.First();
		viewModel.SelectedProduct = productToSelect;

		viewModel.Product.Name.Should().Be(productToSelect.Name);
	}
}
