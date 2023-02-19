// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Avalonia.Core.Products;

namespace Gnomeshade.Avalonia.Core.Tests.Products;

[TestOf(typeof(UnitViewModel))]
public class UnitViewModelTests
{
	[Test]
	public async Task Unit_CreateUnitAsync_ShouldUpdateDataGridView()
	{
		var viewModel = new UnitViewModel(new ActivityService(), new DesignTimeGnomeshadeClient());
		await viewModel.RefreshAsync();
		viewModel.Rows.Should().HaveCount(2);

		var newUnitName = Guid.NewGuid().ToString("N");
		viewModel.Details.Name = newUnitName;
		await viewModel.Details.SaveAsync();

		viewModel.Rows.Should().HaveCount(3).And.ContainSingle(unit => unit.Name == newUnitName);
	}

	[Test]
	public async Task SelectedUnit_ShouldUpdateUnitCreationViewModel()
	{
		var viewModel = new UnitViewModel(new ActivityService(), new DesignTimeGnomeshadeClient());
		await viewModel.RefreshAsync();
		viewModel.Details.Name.Should().BeNullOrWhiteSpace();

		var unitToSelect = viewModel.Rows.First();
		viewModel.SelectedUnit = unitToSelect;

		viewModel.Details.Name.Should().Be(unitToSelect.Name);
	}
}
