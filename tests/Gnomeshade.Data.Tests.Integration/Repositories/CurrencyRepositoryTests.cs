// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Data.Repositories;

using NUnit.Framework;

namespace Gnomeshade.Data.Tests.Integration.Repositories
{
	public class CurrencyRepositoryTests : IDisposable
	{
		private IDbConnection _dbConnection = null!;
		private CurrencyRepository _repository = null!;

		[SetUp]
		public async Task SetUpAsync()
		{
			_dbConnection = await DatabaseInitialization.CreateConnectionAsync().ConfigureAwait(false);
			_repository = new(_dbConnection);
		}

		[TearDown]
		public void Dispose()
		{
			_dbConnection.Dispose();
			_repository.Dispose();
		}

		[Test]
		public async Task GetAllAsync_ShouldReturnExpected()
		{
			var currencies = await _repository.GetAllAsync();
			var expectedCurrencies = new List<string> { "EUR", "USD" };
			currencies.Select(currency => currency.AlphabeticCode).Should().BeEquivalentTo(expectedCurrencies);

			var firstCurrency = currencies.First();
			var currencyById = await _repository.GetByIdAsync(firstCurrency.Id);
			currencyById.Should().BeEquivalentTo(firstCurrency);
		}
	}
}
