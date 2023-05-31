// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Avalonia.Core.Transactions.Transfers;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Tests.Transactions.Transfers;

[TestOf(typeof(TransferViewModel))]
public sealed class TransferViewModelTests
{
	private TransferViewModel _viewModel = null!;

	[SetUp]
	public async Task SetUp()
	{
		_viewModel = new(
			new ActivityService(),
			new DesignTimeGnomeshadeClient(),
			new DesignTimeDialogService(),
			DateTimeZoneProviders.Tzdb,
			Guid.Empty);

		await _viewModel.RefreshAsync();
	}

	[Test]
	public async Task SelectingRow_ShouldUpdateDetails()
	{
		using (new AssertionScope())
		{
			_viewModel.Selected.Should().BeNull();
			_viewModel.Details.Should().NotBeNull();
			_viewModel.Details.CanSave.Should().BeFalse();
			_viewModel.Details.Order.Should().Be(1);
		}

		_viewModel.Selected = _viewModel.Rows.First();
		await _viewModel.UpdateSelection();
		_viewModel.Details.Should().NotBeNull();
		_viewModel.Details.CanSave.Should().BeTrue();

		_viewModel.Selected = null;
		await _viewModel.UpdateSelection();
		_viewModel.Details.CanSave.Should().BeFalse();
	}
}
