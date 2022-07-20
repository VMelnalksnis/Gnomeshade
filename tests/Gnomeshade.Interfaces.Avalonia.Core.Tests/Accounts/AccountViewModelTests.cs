// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.Interfaces.Avalonia.Core.Accounts;
using Gnomeshade.Interfaces.Avalonia.Core.DesignTime;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests.Accounts;

public class AccountViewModelTests
{
	[Test]
	public async Task DataGridView_ShouldBeGrouped()
	{
		var viewModel = new AccountViewModel(new DesignTimeGnomeshadeClient());
		await viewModel.RefreshAsync();

		viewModel.DataGridView.Groups.Should().ContainSingle();
	}
}
