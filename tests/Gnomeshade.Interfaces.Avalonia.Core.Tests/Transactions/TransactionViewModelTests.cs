// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Execution;

using Gnomeshade.Interfaces.Avalonia.Core.DesignTime;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions;

using NodaTime;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests.Transactions;

[TestOf(typeof(TransactionViewModel))]
public class TransactionViewModelTests
{
	private TransactionViewModel _viewModel = null!;

	[SetUp]
	public async Task SetUp()
	{
		_viewModel = await TransactionViewModel.CreateAsync(new DesignTimeGnomeshadeClient(), DateTimeZoneProviders.Tzdb);
	}

	[Test]
	public void Details_ShouldBeUpdatedBySelected()
	{
		using (new AssertionScope())
		{
			_viewModel.Selected.Should().BeNull();
			_viewModel.Details.CanSave.Should().BeFalse();
		}

		_viewModel.Selected = _viewModel.Rows.First();
		_viewModel.Details.Should().NotBeNull();
		_viewModel.Details.CanSave.Should().BeTrue();

		_viewModel.Selected = null;
		_viewModel.Details.CanSave.Should().BeFalse();
	}

	[Test]
	public void DataGridView_ShouldBeEquivalentToRows()
	{
		_viewModel.Rows.Should().HaveCount(_viewModel.DataGridView.Count);
	}

	[Test]
	public void CanDelete_ShouldBeFalseWhenNoneSelected()
	{
		_viewModel.CanDelete.Should().BeFalse();

		_viewModel.Selected = _viewModel.Rows.First();

		_viewModel.CanDelete.Should().BeTrue();
	}

	[Test]
	public void CanDelete_ShouldBeNotifiedOfChange()
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

		canDeleteChanged.Should().BeTrue();
	}

	// todo currently testing against static data, test order matters for deleting
	[Test]
	public async Task X_DeleteSelectedAsync_ShouldDeleteSelectedTransactions()
	{
		var transactionToDelete = _viewModel.Rows.First();
		_viewModel.Selected = transactionToDelete;

		await _viewModel.DeleteSelectedAsync();

		_viewModel.Rows.Should().BeEmpty();
	}
}
