// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;
using Tracking.Finance.Data.TestingHelpers;

namespace Tracking.Finance.Data.Tests.Integration.Repositories
{
	public static class EntityFactory
	{
		private static Product? _product;
		private static (Account First, Account Second)? _accounts;

		public static async Task<Product> GetProductAsync()
		{
			if (_product is not null)
			{
				return _product;
			}

			var dbConnection = await DatabaseInitialization.CreateConnectionAsync().ConfigureAwait(false);
			var repository = new ProductRepository(dbConnection);
			var products = await repository.GetAllAsync().ConfigureAwait(false);
			if (products.Any())
			{
				_product = products.First();
				return _product;
			}

			var faker = new ProductFaker(DatabaseInitialization.TestUser);
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

			var dbConnection = await DatabaseInitialization.CreateConnectionAsync().ConfigureAwait(false);
			var currency = (await new CurrencyRepository(dbConnection).GetAllAsync().ConfigureAwait(false)).First();

			var repository = new AccountRepository(dbConnection);
			var inCurrencyRepository = new AccountInCurrencyRepository(dbConnection);

			var accountFaker = new AccountFaker(DatabaseInitialization.TestUser, currency);
			var firstA = accountFaker.Generate();
			var secondA = accountFaker.GenerateUnique(firstA);

			var firstAccountId = await GetAccountAsync(firstA, repository).ConfigureAwait(false);
			var secondAccountId = await GetAccountAsync(secondA, repository).ConfigureAwait(false);

			var firstInCurrency = new AccountInCurrencyFaker(DatabaseInitialization.TestUser.Id, firstAccountId, currency.Id).Generate();
			var secondInCurrency = new AccountInCurrencyFaker(DatabaseInitialization.TestUser.Id, secondAccountId, currency.Id).Generate();

			await inCurrencyRepository.AddAsync(firstInCurrency).ConfigureAwait(false);
			await inCurrencyRepository.AddAsync(secondInCurrency).ConfigureAwait(false);

			_accounts = (await repository.GetByIdAsync(firstAccountId), await repository.GetByIdAsync(secondAccountId));
			return _accounts.Value;
		}

		private static Task<Guid> GetAccountAsync(Account account, AccountRepository repository)
		{
			return repository.AddAsync(account);
		}
	}
}
