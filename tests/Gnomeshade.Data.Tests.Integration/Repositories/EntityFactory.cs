// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Data.Tests.Integration.Fakers;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.Repositories;

public static class EntityFactory
{
	private static List<CurrencyEntity>? _currencies;
	private static ProductEntity? _product;
	private static (AccountEntity First, AccountEntity Second)? _accounts;

	public static async Task<List<CurrencyEntity>> GetCurrenciesAsync()
	{
		if (_currencies is not null)
		{
			return _currencies.ToList();
		}

		var dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
		_currencies = await new CurrencyRepository(dbConnection).GetAllAsync().ConfigureAwait(false);
		return _currencies;
	}

	public static async Task<ProductEntity> GetProductAsync()
	{
		if (_product is not null)
		{
			return _product;
		}

		var dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
		var repository = new ProductRepository(dbConnection);
		var products = (await repository.GetAllAsync(TestUser.Id)).ToList();
		if (products.Any())
		{
			_product = products.First();
			return _product;
		}

		var faker = new ProductFaker(TestUser);
		var product = faker.Generate();

		var productId = await repository.AddAsync(product).ConfigureAwait(false);
		_product = await repository.GetByIdAsync(productId, TestUser.Id).ConfigureAwait(false);

		return _product;
	}

	public static async Task<(AccountEntity First, AccountEntity Second)> GetAccountsAsync()
	{
		if (_accounts is not null)
		{
			return _accounts.Value;
		}

		var dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
		var currency = (await new CurrencyRepository(dbConnection).GetAllAsync().ConfigureAwait(false)).First();

		var repository = new AccountRepository(dbConnection);
		var inCurrencyRepository = new AccountInCurrencyRepository(dbConnection);
		var counterpartyRepository = new CounterpartyRepository(dbConnection);
		var accountUnitOfWork = new AccountUnitOfWork(dbConnection, repository, inCurrencyRepository, counterpartyRepository);

		var counterParty = new CounterpartyFaker(TestUser.Id).Generate();
		var counterPartyId = await counterpartyRepository.AddAsync(counterParty);
		counterParty = await counterpartyRepository.GetByIdAsync(counterPartyId, TestUser.Id);

		var accountFaker = new AccountFaker(TestUser, counterParty, currency);

		var firstAccount = accountFaker.Generate();
		var firstAccountId = await accountUnitOfWork.AddAsync(firstAccount).ConfigureAwait(false);
		firstAccount = await repository.GetByIdAsync(firstAccountId, TestUser.Id).ConfigureAwait(false);

		var secondAccount = accountFaker.GenerateUnique(firstAccount);
		var secondAccountId = await accountUnitOfWork.AddAsync(secondAccount).ConfigureAwait(false);
		secondAccount = await repository.GetByIdAsync(secondAccountId, TestUser.Id).ConfigureAwait(false);

		_accounts = (firstAccount, secondAccount);
		return _accounts.Value;
	}
}
