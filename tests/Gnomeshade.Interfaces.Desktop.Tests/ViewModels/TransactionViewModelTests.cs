// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
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
		private IGnomeshadeClient _gnomeshadeClient = null!;
		private TransactionViewModel _viewModel = null!;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();

			var mockClient = new Mock<IGnomeshadeClient>();
			mockClient
				.Setup(client => client.GetTransactionsAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
				.ReturnsAsync(new List<TransactionModel>
				{
					new() { Items = new() { new() { SourceAccountId = id1, TargetAccountId = id2 } } },
					new() { Items = new() { new() { SourceAccountId = id2, TargetAccountId = id1 } } },
				});

			mockClient
				.Setup(client => client.GetAccountsAsync())
				.ReturnsAsync(new List<AccountModel>
				{
					new() { Currencies = new() { new() { Id = id1 } } },
					new() { Currencies = new() { new() { Id = id2 } } },
				});

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
			var transactions = _viewModel.Transactions;

			_viewModel.SelectAll.Should().BeFalse();
			transactions.Should().NotContain(transaction => transaction.Selected);

			_viewModel.SelectAll = true;
			transactions.Should().OnlyContain(transaction => transaction.Selected);

			_viewModel.SelectAll = false;
			transactions.Should().NotContain(transaction => transaction.Selected);
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
	}
}
