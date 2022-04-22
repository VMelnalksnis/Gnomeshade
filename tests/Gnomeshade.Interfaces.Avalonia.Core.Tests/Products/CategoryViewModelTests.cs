// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.Avalonia.Core.DesignTime;
using Gnomeshade.Interfaces.Avalonia.Core.Products;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests.Products;

[TestOf(typeof(CategoryViewModel))]
public sealed class CategoryViewModelTests
{
	[Test]
	public async Task Test()
	{
		var viewModel = await CategoryViewModel.CreateAsync(new DesignTimeGnomeshadeClient());

		viewModel.Details.CanSave.Should().BeFalse();
		viewModel.Selected.Should().BeNull();
		viewModel.Rows.Should().NotBeEmpty();

		var rowToSelect = viewModel.Rows.First();
		viewModel.Selected = rowToSelect;

		viewModel.Details.CanSave.Should().BeTrue();

		await viewModel.DeleteSelectedAsync();
		viewModel.Rows.Should().NotContain(row => row.Id == rowToSelect.Id);
	}
}
