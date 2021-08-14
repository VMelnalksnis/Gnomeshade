// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Client.Login;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;
using Gnomeshade.Interfaces.WebApi.Models.Importing;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Desktop.ViewModels.Design
{
	/// <summary>
	/// An implementation of <see cref="IGnomeshadeClient"/> for use during design time.
	/// </summary>
	public sealed class DesignTimeGnomeshadeClient : IGnomeshadeClient
	{
		private static readonly List<Currency> _currencies;
		private static readonly List<Account> _accounts;
		private static readonly List<UnitModel> _units;
		private static readonly List<ProductModel> _products;
		private static readonly List<TransactionModel> _transactions;

		static DesignTimeGnomeshadeClient()
		{
			var euro = new Currency { Id = Guid.NewGuid(), Name = "Euro", AlphabeticCode = "EUR" };
			var usd = new Currency { Id = Guid.NewGuid(), Name = "United States Dollar", AlphabeticCode = "USD" };
			_currencies = new() { euro, usd };

			var cash = new Account
			{
				Id = Guid.NewGuid(),
				Name = "Cash",
				PreferredCurrency = euro,
				Currencies = new() { new() { Id = Guid.NewGuid(), Currency = euro } },
			};
			var spending = new Account
			{
				Id = Guid.NewGuid(),
				Name = "Spending",
				PreferredCurrency = euro,
				Currencies = new() { new() { Id = Guid.NewGuid(), Currency = euro } },
			};
			_accounts = new() { cash, spending };

			var kilogram = new UnitModel { Id = Guid.NewGuid(), Name = "Kilogram" };
			_units = new() { kilogram };

			var bread = new ProductModel { Id = Guid.NewGuid(), Name = "Bread" };
			var milk = new ProductModel { Id = Guid.NewGuid(), Name = "Milk" };
			_products = new() { bread, milk };

			var transaction = new TransactionModel
			{
				Id = Guid.Empty,
				Date = DateTimeOffset.Now,
				Description = "Some transaction description",
				Items = new()
				{
					new()
					{
						Id = Guid.NewGuid(),
						TransactionId = Guid.Empty,
						SourceAmount = 125.35m,
						TargetAmount = 125.35m,
						SourceAccountId = spending.Currencies.Single().Id,
						TargetAccountId = cash.Currencies.Single().Id,
						Product = bread,
						Amount = 1,
					},
					new()
					{
						Id = Guid.NewGuid(),
						TransactionId = Guid.Empty,
						SourceAmount = 1.95m,
						TargetAmount = 1.95m,
						SourceAccountId = spending.Currencies.Single().Id,
						TargetAccountId = cash.Currencies.Single().Id,
						Product = milk,
						Amount = 2,
					},
				},
			};

			_transactions = new() { transaction };
		}

		/// <inheritdoc />
		public Task<LoginResult> LogInAsync(LoginModel login) => throw new NotImplementedException();

		/// <inheritdoc />
		public Task LogOutAsync() => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<UserModel> InfoAsync() => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<Counterparty> GetMyCounterpartyAsync() => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<Guid> CreateTransactionAsync(TransactionCreationModel transaction) =>
			throw new NotImplementedException();

		/// <inheritdoc />
		public Task<Guid> PutTransactionItemAsync(Guid transactionId, TransactionItemCreationModel item)
		{
			var transactionWithItem = _transactions.Single(t => t.Id == transactionId);
			if (item.Id is not null)
			{
				var existingItem = transactionWithItem.Items.SingleOrDefault(i => i.Id == item.Id.Value);
				if (existingItem is not null)
				{
					transactionWithItem.Items.Remove(existingItem);
				}
			}

			var itemModel = new TransactionItemModel
			{
				Id = Guid.NewGuid(),
				TransactionId = transactionWithItem.Id,
				SourceAccountId = item.SourceAccountId!.Value,
				TargetAccountId = item.TargetAccountId!.Value,
				Product = _products.Single(p => p.Id == item.ProductId),
			};

			transactionWithItem.Items.Add(itemModel);
			return Task.FromResult(itemModel.Id);
		}

		/// <inheritdoc />
		public Task<TransactionModel> GetTransactionAsync(Guid id)
		{
			return Task.FromResult(_transactions.Single(transaction => transaction.Id == id));
		}

		/// <inheritdoc />
		public Task<List<TransactionModel>> GetTransactionsAsync(DateTimeOffset? from, DateTimeOffset? to)
		{
			return Task.FromResult(_transactions.ToList());
		}

		/// <inheritdoc />
		public Task DeleteTransactionAsync(Guid id)
		{
			_transactions.Remove(_transactions.Single(transaction => transaction.Id == id));
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task DeleteTransactionItemAsync(Guid id)
		{
			var transactionWithItem = _transactions.Single(t => t.Items.Select(item => item.Id).Contains(id));
			transactionWithItem.Items.Remove(transactionWithItem.Items.Single(item => item.Id == id));
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task<Account> GetAccountAsync(Guid id)
		{
			return Task.FromResult(_accounts.Single(account => account.Id == id));
		}

		/// <inheritdoc />
		public Task<Account?> FindAccountAsync(string name)
		{
			var foundAccount = _accounts.SingleOrDefault(account => account.Name.ToUpperInvariant() == name);
			return Task.FromResult(foundAccount);
		}

		/// <inheritdoc />
		public Task<List<Account>> GetAccountsAsync()
		{
			return Task.FromResult(_accounts.ToList());
		}

		/// <inheritdoc />
		public Task<List<Account>> GetActiveAccountsAsync()
		{
			return Task.FromResult(_accounts.Where(account => !account.Disabled).ToList());
		}

		/// <inheritdoc />
		public Task<Guid> CreateAccountAsync(AccountCreationModel account) => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<Guid> AddCurrencyToAccountAsync(Guid id, AccountInCurrencyCreationModel currency) =>
			throw new NotImplementedException();

		/// <inheritdoc />
		public Task<List<Currency>> GetCurrenciesAsync()
		{
			return Task.FromResult(_currencies.ToList());
		}

		/// <inheritdoc />
		public Task<List<ProductModel>> GetProductsAsync()
		{
			return Task.FromResult(_products.ToList());
		}

		/// <inheritdoc />
		public Task<List<UnitModel>> GetUnitsAsync()
		{
			return Task.FromResult(_units.ToList());
		}

		/// <inheritdoc />
		public Task<Guid> PutProductAsync(ProductCreationModel product) => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<Guid> CreateUnitAsync(UnitCreationModel unit) => throw new NotImplementedException();

		/// <inheritdoc />
		public Task<AccountReportResult> Import(Stream content, string name) => throw new NotImplementedException();
	}
}
