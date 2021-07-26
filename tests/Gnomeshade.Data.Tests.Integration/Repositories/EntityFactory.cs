// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Models;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Data.TestingHelpers;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.Repositories
{
	public static class EntityFactory
	{
		private static List<Currency>? _currencies;
		private static Product? _product;
		private static (Account First, Account Second)? _accounts;

		public static async Task<List<Currency>> GetCurrenciesAsync()
		{
			if (_currencies is not null)
			{
				return _currencies.ToList();
			}

			var dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
			_currencies = await new CurrencyRepository(dbConnection).GetAllAsync().ConfigureAwait(false);
			return _currencies;
		}

		public static async Task<Product> GetProductAsync()
		{
			if (_product is not null)
			{
				return _product;
			}

			var dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
			var repository = new ProductRepository(dbConnection);
			var products = await repository.GetAllAsync().ConfigureAwait(false);
			if (products.Any())
			{
				_product = products.First();
				return _product;
			}

			var faker = new ProductFaker(TestUser);
			var product = faker.Generate();

			var productId = await repository.AddAsync(product).ConfigureAwait(false);
			_product = await repository.GetByIdAsync(productId).ConfigureAwait(false);

			return _product;
		}

		public static async Task<(Account First, Account Second)> GetAccountsAsync()
		{
			if (_accounts is not null)
			{
				return _accounts.Value;
			}

			var dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
			var currency = (await new CurrencyRepository(dbConnection).GetAllAsync().ConfigureAwait(false)).First();

			var repository = new AccountRepository(dbConnection);
			var inCurrencyRepository = new AccountInCurrencyRepository(dbConnection);
			var accountUnitOfWork = new AccountUnitOfWork(dbConnection, repository, inCurrencyRepository);

			var accountFaker = new AccountFaker(TestUser, currency);

			var firstAccount = accountFaker.Generate();
			var firstAccountId = await accountUnitOfWork.AddAsync(firstAccount, currency).ConfigureAwait(false);
			firstAccount = await repository.GetByIdAsync(firstAccountId).ConfigureAwait(false);

			var secondAccount = accountFaker.GenerateUnique(firstAccount);
			var secondAccountId = await accountUnitOfWork.AddAsync(secondAccount, currency).ConfigureAwait(false);
			secondAccount = await repository.GetByIdAsync(secondAccountId).ConfigureAwait(false);

			_accounts = (firstAccount, secondAccount);
			return _accounts.Value;
		}
	}
}
