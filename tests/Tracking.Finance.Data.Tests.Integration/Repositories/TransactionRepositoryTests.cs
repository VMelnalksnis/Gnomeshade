// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;

using static Tracking.Finance.Data.Tests.Integration.DatabaseInitialization;

namespace Tracking.Finance.Data.Tests.Integration.Repositories
{
	public class TransactionRepositoryTests : IDisposable
	{
		private IDbConnection _dbConnection = null!;
		private TransactionRepository _repository = null!;
		private TransactionItemRepository _itemRepository = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
			_repository = new(_dbConnection);
			_itemRepository = new(_dbConnection);
		}

		[TearDown]
		public void Dispose()
		{
			_dbConnection.Dispose();
			_repository.Dispose();
			_itemRepository.Dispose();
		}

		[Test]
		public async Task FindByIdAsync_ShouldReturnNullIfDoesNotExist()
		{
			var id = Guid.NewGuid();
			var transaction = await _repository.FindByIdAsync(id);
			transaction.Should().BeNull();
		}

		[Test]
		public async Task FindByImportHashAsync_ShouldReturnNullIfDoesNotExist()
		{
			var importHash = await new Transaction().GetHashAsync();
			var transaction = await _repository.FindByImportHashAsync(importHash);
			transaction.Should().BeNull();
		}
	}
}
