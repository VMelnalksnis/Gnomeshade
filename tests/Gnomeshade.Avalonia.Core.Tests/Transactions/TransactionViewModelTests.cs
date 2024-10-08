﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Avalonia.Core.Transactions;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Tests.Transactions;

[TestOf(typeof(TransactionViewModel))]
public class TransactionViewModelTests
{
	private TransactionViewModel _viewModel = null!;

	[SetUp]
	public async Task SetUp()
	{
		var activityService = new StubbedActivityService();
		_viewModel = new(
			activityService,
			new DesignTimeGnomeshadeClient(),
			new DesignTimeDialogService(),
			SystemClock.Instance,
			DateTimeZoneProviders.Tzdb);
		await _viewModel.RefreshAsync();
	}

	[Test]
	public void Details_ShouldBeUpdatedBySelected()
	{
		using (new AssertionScope())
		{
			_viewModel.Selected.Should().BeNull();
			_viewModel.Details.Properties.Description.Should().BeNull();
		}

		_viewModel.UpdateSelectedItems.Execute(new ArrayList { _viewModel.Rows.First() });

		_viewModel.Details.Should().NotBeNull();
		_viewModel.Details.Properties.Description.Should().NotBeNull();

		_viewModel.Selected = null;
		_viewModel.UpdateSelectedItems.Execute(new ArrayList());

		_viewModel.Details.Properties.Description.Should().BeNull();
	}

	[Test]
	public void DataGridView_ShouldBeEquivalentToRows()
	{
		_viewModel.Rows.Should().HaveCount(_viewModel.DataGridView.Count);
	}

	[Test]
	public async Task CanDelete_ShouldBeFalseWhenNoneSelected()
	{
		_viewModel.CanDelete.Should().BeFalse();

		_viewModel.Selected = _viewModel.Rows.First();
		await _viewModel.UpdateSelection();

		_viewModel.CanDelete.Should().BeTrue();
	}

	[Test]
	public async Task CanDelete_ShouldBeNotifiedOfChange()
	{
		var canDeleteChanged = false;
		_viewModel.PropertyChanged += (_, args) =>
		{
			if (args.PropertyName == nameof(TransactionViewModel.CanDelete))
			{
				canDeleteChanged = true;
			}
		};

		_viewModel.Selected = _viewModel.Rows.First();
		await _viewModel.UpdateSelection();

		canDeleteChanged.Should().BeTrue();
	}
}
