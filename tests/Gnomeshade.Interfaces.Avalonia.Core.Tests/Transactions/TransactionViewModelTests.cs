// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.Avalonia.Core.DesignTime;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Items;
using Gnomeshade.Interfaces.WebApi.Client;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests.Transactions;

public class TransactionViewModelTests
{
	private readonly IGnomeshadeClient _gnomeshadeClient = new DesignTimeGnomeshadeClient();
	private TransactionViewModel _viewModel = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_viewModel = await TransactionViewModel.CreateAsync(new DesignTimeGnomeshadeClient());
	}

	[Test]
	public void CanDelete_ShouldBeFalseWhenNoneSelected()
	{
		_viewModel.CanDelete.Should().BeFalse();

		_viewModel.SelectedOverview = _viewModel.Transactions.First();

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

		_viewModel.SelectedOverview = _viewModel.Transactions.First();

		canDeleteChanged.Should().BeTrue();
	}

	[Test]
	public async Task DeleteSelectedAsync_ShouldDeleteSelectedTransactions()
	{
		var transactionToDelete = _viewModel.Transactions.First();
		_viewModel.SelectedOverview = transactionToDelete;
		await _viewModel.DeleteSelectedAsync();

		await FluentActions
			.Awaiting(() => _gnomeshadeClient.GetTransactionAsync(transactionToDelete.Id))
			.Should()
			.ThrowAsync<InvalidOperationException>();
	}
}
