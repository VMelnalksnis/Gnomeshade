// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Execution;

using Gnomeshade.Interfaces.Desktop.ViewModels;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.V1_0.Accounts;
using Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

using Moq;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.Desktop.Tests.ViewModels
{
	public class TransactionViewModelTests
	{
		private Mock<IGnomeshadeClient> _gnomeshadeClientMock = null!;
		private IGnomeshadeClient _gnomeshadeClient = null!;
		private TransactionViewModel _viewModel = null!;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			var transactionId1 = Guid.NewGuid();
			var transactionId2 = Guid.NewGuid();

			var accountId1 = Guid.NewGuid();
			var accountId2 = Guid.NewGuid();

			var mockClient = new Mock<IGnomeshadeClient>();
			mockClient
				.Setup(client => client.GetTransactionsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
				.ReturnsAsync(new List<TransactionModel>
				{
					new()
					{
						Id = transactionId1,
						Items = new() { new() { SourceAccountId = accountId1, TargetAccountId = accountId2 } },
					},
					new()
					{
						Id = transactionId2,
						Items = new() { new() { SourceAccountId = accountId2, TargetAccountId = accountId1 } },
					},
				});

			mockClient
				.Setup(client => client.GetAccountsAsync())
				.ReturnsAsync(new List<AccountModel>
				{
					new() { Currencies = new() { new() { Id = accountId1 } } },
					new() { Currencies = new() { new() { Id = accountId2 } } },
				});

			_gnomeshadeClientMock = mockClient;
			_gnomeshadeClient = mockClient.Object;
		}

		[SetUp]
		public void SetUp()
		{
			_viewModel = new(_gnomeshadeClient);
		}

		[Test]
		public void SelectAll_ShouldAffectAllTransactions()
		{
			_viewModel.SelectAll.Should().BeFalse();
			_viewModel.Transactions.Should().NotContain(transaction => transaction.Selected);

			_viewModel.SelectAll = true;
			_viewModel.Transactions.Should().OnlyContain(transaction => transaction.Selected);

			_viewModel.SelectAll = false;
			_viewModel.Transactions.Should().NotContain(transaction => transaction.Selected);
		}

		[Test]
		public async Task SelectAll_ShouldResetAfterSearch()
		{
			_viewModel.SelectAll = true;

			await _viewModel.SearchAsync();

			using (new AssertionScope())
			{
				_viewModel.SelectAll.Should().BeFalse();
				_viewModel.Transactions.Should().NotContain(transaction => transaction.Selected);
			}
		}

		[Test]
		public void CanDelete_ShouldBeFalseWhenNoneSelected()
		{
			_viewModel.CanDelete.Should().BeFalse();

			_viewModel.Transactions.First().Selected = true;

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

			var firstTransaction = _viewModel.Transactions.First();
			firstTransaction.Selected = !firstTransaction.Selected;

			canDeleteChanged.Should().BeTrue();
		}

		[Test]
		public async Task DeleteSelectedAsync_ShouldDeleteSelectedTransactions()
		{
			var deletedIds = new List<Guid>();
			_gnomeshadeClientMock
				.Setup(client => client.DeleteTransactionAsync(It.IsAny<Guid>()))
				.Callback<Guid>(id => deletedIds.Add(id));

			var transactionToDelete = _viewModel.Transactions.First();
			transactionToDelete.Selected = true;
			await _viewModel.DeleteSelectedAsync();

			deletedIds.Should().ContainSingle().Which.Should().Be(transactionToDelete.Id);
		}
	}
}
