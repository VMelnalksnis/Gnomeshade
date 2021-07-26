// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Data.Repositories;

using NUnit.Framework;

namespace Gnomeshade.Data.Tests.Integration.Repositories
{
	public class OwnerRepositoryTests : IDisposable
	{
		private IDbConnection _dbConnection = null!;
		private OwnerRepository _ownerRepository = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_dbConnection = await DatabaseInitialization.CreateConnectionAsync().ConfigureAwait(false);
			_ownerRepository = new(_dbConnection);
		}

		[TearDown]
		public void Dispose()
		{
			_dbConnection.Dispose();
			_ownerRepository.Dispose();
		}

		[Test]
		public async Task AddAsync_ShouldGenerateGuid()
		{
			var id = await _ownerRepository.AddAsync();
			id.Should().NotBe(Guid.Empty);
		}

		[Test]
		public async Task GetAllAsync()
		{
			var owners = await _ownerRepository.GetAllAsync();
			owners.Should().OnlyHaveUniqueItems();
		}
	}
}
