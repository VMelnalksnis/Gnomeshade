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

namespace Tracking.Finance.Data.Tests.Integration.Repositories
{
	public class OwnerRepositoryTests
	{
		private IDbConnection _dbConnection = null!;
		private OwnerRepository _ownerRepository = null!;

		[SetUp]
		public async Task SetUp()
		{
			_dbConnection = await DatabaseInitialization.CreateConnection();
			_ownerRepository = new(_dbConnection);
		}

		[Test]
		public async Task AddAsync_ShouldGenerateGuid()
		{
			var owner = new Owner();

			var id = await _ownerRepository.AddAsync(owner);
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
