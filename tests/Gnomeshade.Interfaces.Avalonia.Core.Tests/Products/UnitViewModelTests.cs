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

using static Gnomeshade.Interfaces.Avalonia.Core.Products.UnitViewModel;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests.Products;

[TestOf(typeof(UnitViewModel))]
public class UnitViewModelTests
{
	[Test]
	public async Task Unit_CreateUnitAsync_ShouldUpdateDataGridView()
	{
		var viewModel = await CreateAsync(new DesignTimeGnomeshadeClient());
		viewModel.Units.Should().HaveCount(2);

		var newUnitName = Guid.NewGuid().ToString("N");
		viewModel.Unit.Name = newUnitName;
		await viewModel.Unit.CreateUnitAsync();

		viewModel.Units.Should().HaveCount(3).And.ContainSingle(product => product.Name == newUnitName);
	}

	[Test]
	public async Task SelectedUnit_ShouldUpdateUnitCreationViewModel()
	{
		var viewModel = await CreateAsync(new DesignTimeGnomeshadeClient());
		viewModel.Unit.Name.Should().BeNullOrWhiteSpace();

		var unitToSelect = viewModel.Units.First();
		viewModel.SelectedUnit = unitToSelect;

		viewModel.Unit.Name.Should().Be(unitToSelect.Name);
	}
}
