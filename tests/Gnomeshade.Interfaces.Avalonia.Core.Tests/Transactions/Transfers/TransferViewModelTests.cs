// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.Avalonia.Core.DesignTime;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Transfers;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests.Transactions.Transfers;

[TestOf(typeof(TransferViewModel))]
public class TransferViewModelTests
{
	private TransferViewModel _viewModel = null!;

	[SetUp]
	public async Task SetUp()
	{
		_viewModel = await TransferViewModel.CreateAsync(new DesignTimeGnomeshadeClient(), Guid.Empty);
	}

	[Test]
	public void SelectingRow_ShouldUpdateDetails()
	{
		using (new AssertionScope())
		{
			_viewModel.Selected.Should().BeNull();
			_viewModel.Details.Should().NotBeNull();
			_viewModel.Details.CanSave.Should().BeFalse();
		}

		_viewModel.Selected = _viewModel.Rows.First();
		_viewModel.Details.Should().NotBeNull();
		_viewModel.Details.CanSave.Should().BeTrue();

		_viewModel.Selected = null;
		_viewModel.Details.CanSave.Should().BeFalse();
	}
}
