// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Avalonia.Core.Products;

namespace Gnomeshade.Avalonia.Core.Tests.Products;

[TestOf(typeof(CategoryViewModel))]
public sealed class CategoryViewModelTests
{
	[Test]
	public async Task Test()
	{
		var viewModel = new CategoryViewModel(new StubbedActivityService(), new DesignTimeGnomeshadeClient());
		await viewModel.RefreshAsync();

		viewModel.Details.CanSave.Should().BeFalse();
		viewModel.Selected.Should().BeNull();
		viewModel.Rows.Should().NotBeEmpty();

		var rowToSelect = viewModel.Rows.First();
		viewModel.Selected = rowToSelect;
		await viewModel.UpdateSelection();

		viewModel.Details.CanSave.Should().BeTrue();

		viewModel.DeleteSelected.Execute(null);
		viewModel.Rows.Should().NotContain(row => row.Id == rowToSelect.Id);
	}
}
