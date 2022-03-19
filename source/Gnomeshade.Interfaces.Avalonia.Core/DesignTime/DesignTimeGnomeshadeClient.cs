// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;
using Gnomeshade.Interfaces.WebApi.Models.Importing;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Tags;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.DesignTime;

/// <summary>An implementation of <see cref="IGnomeshadeClient"/> for use during design time.</summary>
public sealed class DesignTimeGnomeshadeClient : IGnomeshadeClient
{
	private static readonly List<Currency> _currencies;
	private static readonly List<Counterparty> _counterparties;
	private static readonly List<Account> _accounts;
	private static readonly List<Unit> _units;
	private static readonly List<Product> _products;
	private static readonly List<Transaction> _transactions;

	static DesignTimeGnomeshadeClient()
	{
		var euro = new Currency { Id = Guid.NewGuid(), Name = "Euro", AlphabeticCode = "EUR" };
		var usd = new Currency { Id = Guid.NewGuid(), Name = "United States Dollar", AlphabeticCode = "USD" };
		_currencies = new() { euro, usd };

		var counterparty = new Counterparty { Id = Guid.Empty, Name = "John Doe" };
		_counterparties = new() { counterparty };

		var cash = new Account
		{
			Id = Guid.Empty,
			Name = "Cash",
			CounterpartyId = counterparty.Id,
			PreferredCurrency = euro,
			Currencies = new() { new() { Id = Guid.NewGuid(), Currency = euro } },
		};
		var spending = new Account
		{
			Id = Guid.NewGuid(),
			Name = "Spending",
			CounterpartyId = counterparty.Id,
			PreferredCurrency = euro,
			Currencies = new() { new() { Id = Guid.NewGuid(), Currency = euro } },
		};
		_accounts = new() { cash, spending };

		var kilogram = new Unit { Id = Guid.NewGuid(), Name = "Kilogram" };
		var gram = new Unit { Id = Guid.NewGuid(), Name = "Gram", ParentUnitId = kilogram.Id, Multiplier = 1000m };
		_units = new() { kilogram, gram };

		var bread = new Product { Id = Guid.NewGuid(), Name = "Bread" };
		var milk = new Product { Id = Guid.NewGuid(), Name = "Milk" };
		_products = new() { bread, milk };

		var transaction = new Transaction
		{
			Id = Guid.Empty,
			BookedAt = DateTimeOffset.UtcNow,
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
	public Task<LoginResult> LogInAsync(Login login) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task SocialRegister(string accessToken) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task LogOutAsync() => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<Counterparty> GetMyCounterpartyAsync() =>
		Task.FromResult(_counterparties.Single(counterparty => counterparty.Id == Guid.Empty));

	/// <inheritdoc />
	public Task<Counterparty> GetCounterpartyAsync(Guid id) =>
		Task.FromResult(_counterparties.Single(counterparty => counterparty.Id == id));

	/// <inheritdoc />
	public Task<List<Counterparty>> GetCounterpartiesAsync() => Task.FromResult(_counterparties.ToList());

	/// <inheritdoc />
	public Task<Guid> CreateCounterpartyAsync(CounterpartyCreationModel counterparty) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task PutCounterpartyAsync(Guid id, CounterpartyCreationModel counterparty) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task MergeCounterpartiesAsync(Guid targetId, Guid sourceId) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<Guid> CreateTransactionAsync(TransactionCreationModel transaction) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task PutTransactionAsync(Guid id, TransactionCreationModel transaction) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task PutTransactionItemAsync(Guid id, Guid transactionId, TransactionItemCreationModel item)
	{
		var transactionWithItem = _transactions.Single(t => t.Id == transactionId);
		var existingItem = transactionWithItem.Items.SingleOrDefault(i => i.Id == id);
		if (existingItem is not null)
		{
			transactionWithItem.Items.Remove(existingItem);
		}

		var itemModel = new TransactionItem
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
	public Task<Transaction> GetTransactionAsync(Guid id)
	{
		return Task.FromResult(_transactions.Single(transaction => transaction.Id == id));
	}

	/// <inheritdoc />
	public Task<TransactionItem> GetTransactionItemAsync(Guid id)
	{
		var selectedItem = _transactions
			.SelectMany(transaction => transaction.Items)
			.Single(item => item.Id == id);

		return Task.FromResult(selectedItem);
	}

	/// <inheritdoc />
	public Task<List<Transaction>> GetTransactionsAsync(DateTimeOffset? from, DateTimeOffset? to)
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
	public Task<List<Tag>> GetTransactionItemTagsAsync(Guid id)
	{
		return Task.FromResult(new List<Tag> { new() { Name = "test" }, new() { Name = "other" } });
	}

	/// <inheritdoc />
	public Task TagTransactionItemAsync(Guid id, Guid tagId) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task UntagTransactionItemAsync(Guid id, Guid tagId) => throw new NotImplementedException();

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
	public Task PutAccountAsync(Guid id, AccountCreationModel account) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<Guid> AddCurrencyToAccountAsync(Guid id, AccountInCurrencyCreationModel currency) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task<List<Currency>> GetCurrenciesAsync()
	{
		return Task.FromResult(_currencies.ToList());
	}

	/// <inheritdoc />
	public Task<List<Product>> GetProductsAsync()
	{
		return Task.FromResult(_products.ToList());
	}

	/// <inheritdoc />
	public Task<Product> GetProductAsync(Guid id)
	{
		return Task.FromResult(_products.Single(product => product.Id == id));
	}

	/// <inheritdoc />
	public Task<Unit> GetUnitAsync(Guid id)
	{
		return Task.FromResult(_units.Single(unit => unit.Id == id));
	}

	/// <inheritdoc />
	public Task<List<Unit>> GetUnitsAsync()
	{
		return Task.FromResult(_units.ToList());
	}

	/// <inheritdoc />
	public Task PutProductAsync(Guid id, ProductCreationModel product)
	{
		var model = new Product
		{
			Id = id,
			Name = product.Name!,
			Description = product.Description,
			UnitId = product.UnitId,
		};

		var existingProduct = _products.SingleOrDefault(p => p.Id == id);
		if (existingProduct is not null)
		{
			_products.Remove(existingProduct);
		}

		_products.Add(model);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task PutUnitAsync(Guid id, UnitCreationModel unit)
	{
		var model = new Unit
		{
			Id = id,
			Name = unit.Name!,
			ParentUnitId = unit.ParentUnitId,
			Multiplier = unit.Multiplier,
		};

		var existingUnit = _units.SingleOrDefault(u => u.Id == id);
		if (existingUnit is not null)
		{
			_units.Remove(existingUnit);
		}

		_units.Add(model);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task<AccountReportResult> Import(Stream content, string name) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<List<Tag>> GetTagsAsync()
	{
		return Task.FromResult<List<Tag>>(new() { new() { Name = "Foo" }, new() { Name = "Bar" } });
	}

	/// <inheritdoc />
	public Task<Tag> GetTagAsync(Guid id) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<Guid> CreateTagAsync(TagCreation tag) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task PutTagAsync(Guid id, TagCreation tag) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task DeleteTagAsync(Guid id) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<List<Tag>> GetTagTagsAsync(Guid id) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task TagTagAsync(Guid id, Guid tagId) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task UntagTagAsync(Guid id, Guid tagId) => throw new NotImplementedException();
}
