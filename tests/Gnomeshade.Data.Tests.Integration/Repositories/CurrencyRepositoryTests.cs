// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Repositories;

namespace Gnomeshade.Data.Tests.Integration.Repositories;

public sealed class CurrencyRepositoryTests
{
	private DbConnection _dbConnection = null!;
	private CurrencyRepository _repository = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_dbConnection = await DatabaseInitialization.CreateConnectionAsync().ConfigureAwait(false);
		_repository = new(_dbConnection);
	}

	[Test]
	public async Task GetAllAsync_ShouldReturnExpected()
	{
		var currencies = await _repository.GetAllAsync();
		var expectedCurrencies = new List<string>
		{
			"CZK", "EUR", "GBP", "HRK", "LVL", "PLN", "RUB", "USD", "HRD", "SEK",
		};
		currencies.Select(currency => currency.AlphabeticCode).Should().BeEquivalentTo(expectedCurrencies);

		var firstCurrency = currencies.First();
		var currencyById = await _repository.GetByIdAsync(firstCurrency.Id);
		currencyById.Should().BeEquivalentTo(firstCurrency);
	}
}
