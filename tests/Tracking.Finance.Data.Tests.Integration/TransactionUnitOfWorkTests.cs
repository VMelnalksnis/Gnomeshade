﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Equivalency;

using NUnit.Framework;

using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;
using Tracking.Finance.Data.TestingHelpers;
using Tracking.Finance.Data.Tests.Integration.Repositories;

using static Tracking.Finance.Data.Tests.Integration.DatabaseInitialization;

namespace Tracking.Finance.Data.Tests.Integration
{
	public sealed class TransactionUnitOfWorkTests : IDisposable
	{
		private IDbConnection _dbConnection = null!;
		private TransactionRepository _repository = null!;
		private TransactionItemRepository _itemRepository = null!;
		private TransactionUnitOfWork _unitOfWork = null!;

		[SetUp]
		public async Task OneTimeSetupAsync()
		{
			_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
			_repository = new(_dbConnection);
			_itemRepository = new(_dbConnection);
			_unitOfWork = new(_dbConnection, _repository, _itemRepository);
		}

		[TearDown]
		public void Dispose()
		{
			_dbConnection.Dispose();
			_repository.Dispose();
			_itemRepository.Dispose();
			_unitOfWork.Dispose();
		}

		[Test]
		public async Task AddGetDelete_WithoutTransaction()
		{
			var product = await EntityFactory.GetProductAsync();
			var (first, second) = await EntityFactory.GetAccountsAsync();

			var transactionToAdd = new TransactionFaker(TestUser).Generate();
			var importHash = await transactionToAdd.GetHashAsync();
			transactionToAdd = transactionToAdd with { ImportHash = importHash };

			var transactionItemToAdd = new TransactionItemFaker(
					TestUser,
					transactionToAdd,
					first.Currencies.Single(),
					second.Currencies.Single(),
					product)
				.Generate();

			var transactionId = await _unitOfWork.AddAsync(transactionToAdd, new[] { transactionItemToAdd });

			var getTransaction = await _repository.GetByIdAsync(transactionId);
			var findTransaction = await _repository.FindByIdAsync(getTransaction.Id);
			var findImportTransaction = await _repository.FindByImportHashAsync(importHash);
			var allTransactions = await _repository.GetAllAsync(DateTimeOffset.Now.AddMonths(-1), DateTimeOffset.Now);

			getTransaction.Items.Should().ContainSingle().Which.Product.Should().BeEquivalentTo(product);
			findTransaction.Should().BeEquivalentTo(getTransaction, Options);
			findImportTransaction.Should().BeEquivalentTo(getTransaction, Options);
			allTransactions.Should().ContainSingle().Which.Should().BeEquivalentTo(getTransaction, Options);

			await _unitOfWork.DeleteAsync(getTransaction);
		}

		private static EquivalencyAssertionOptions<Transaction> Options(
			EquivalencyAssertionOptions<Transaction> options)
		{
			return options.ComparingByMembers<Transaction>().ComparingByMembers<TransactionItem>();
		}
	}
}
