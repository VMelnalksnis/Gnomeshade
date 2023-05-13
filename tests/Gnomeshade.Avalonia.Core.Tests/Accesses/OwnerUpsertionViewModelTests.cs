// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Accesses;
using Gnomeshade.Avalonia.Core.DesignTime;

namespace Gnomeshade.Avalonia.Core.Tests.Accesses;

[TestOf(typeof(OwnerUpsertionViewModel))]
public sealed class OwnerUpsertionViewModelTests
{
	[Test]
	public async Task CreateNewOwner()
	{
		var client = new DesignTimeGnomeshadeClient();
		var viewModel = new OwnerUpsertionViewModel(new ActivityService(), client, null);
		await viewModel.RefreshAsync();

		viewModel.SelectedAccess = viewModel.Accesses.First();
		viewModel.SelectedCounterparty = viewModel.Counterparties.First();

		viewModel.CanSaveRow.Should().BeTrue();
		viewModel.SaveRow();
		using (new AssertionScope())
		{
			viewModel.Ownerships.Should().ContainSingle();
			viewModel.SelectedRow.Should().BeNull();
		}

		viewModel.SelectedAccess = viewModel.Accesses.Skip(1).First();
		viewModel.SelectedCounterparty = viewModel.Counterparties.First();
		viewModel.SaveRow();

		viewModel.Ownerships.Should().HaveCount(2);

		viewModel.Name = "Foo";
		viewModel.CanSave.Should().BeTrue();

		await viewModel.SaveAsync();
		await viewModel.RefreshAsync();

		viewModel.SelectedRow = viewModel.Ownerships.Last();
		viewModel.RemoveRow();
		viewModel.Ownerships.Should().ContainSingle();

		await viewModel.SaveAsync();
		await viewModel.RefreshAsync();

		viewModel.Ownerships.Should().ContainSingle();
	}
}
