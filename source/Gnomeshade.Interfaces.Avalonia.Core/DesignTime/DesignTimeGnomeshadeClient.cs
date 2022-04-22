// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;
using Gnomeshade.Interfaces.WebApi.Models.Importing;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

using NodaTime;

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
	private static readonly List<Transfer> _transfers;
	private static readonly List<Purchase> _purchases;
	private static readonly List<Link> _links;
	private static readonly List<KeyValuePair<Guid, Guid>> _transactionLinks;

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
			BookedAt = SystemClock.Instance.GetCurrentInstant(),
			Description = "Some transaction description",
		};

		_transactions = new() { transaction };
		_transfers = new()
		{
			new()
			{
				Id = Guid.NewGuid(),
				TransactionId = transaction.Id,
				SourceAmount = 125.35m,
				SourceAccountId = spending.Currencies.Single().Id,
				TargetAmount = 125.35m,
				TargetAccountId = cash.Currencies.Single().Id,
			},
			new()
			{
				Id = Guid.NewGuid(),
				TransactionId = transaction.Id,
				SourceAmount = 1.95m,
				SourceAccountId = spending.Currencies.Single().Id,
				TargetAmount = 1.95m,
				TargetAccountId = cash.Currencies.Single().Id,
			},
		};

		_purchases = new()
		{
			new()
			{
				Id = Guid.NewGuid(),
				TransactionId = transaction.Id,
				Price = 1,
				CurrencyId = euro.Id,
				Amount = 2,
				ProductId = milk.Id,
			},
			new()
			{
				Id = Guid.NewGuid(),
				TransactionId = transaction.Id,
				Price = 2.35m,
				CurrencyId = euro.Id,
				Amount = 500,
				ProductId = bread.Id,
				DeliveryDate = SystemClock.Instance.GetCurrentInstant(),
			},
		};

		_links = new()
		{
			new() { Id = Guid.Empty, Uri = "https://localhost/documents/1" },
			new() { Id = Guid.NewGuid(), Uri = "https://localhost/documents/1" },
		};

		_transactionLinks = new()
		{
			new(Guid.Empty, Guid.Empty),
			new(Guid.Empty, _links.Last().Id),
		};
	}

	/// <inheritdoc />
	public Task<LoginResult> LogInAsync(Login login) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task SocialRegister(string accessToken) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task LogOutAsync() => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<List<Link>> GetLinksAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_links.ToList());
	}

	/// <inheritdoc />
	public Task<Link> GetLinkAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_links.Single(link => link.Id == id));
	}

	/// <inheritdoc />
	public Task PutLinkAsync(Guid id, LinkCreation link)
	{
		var existingLink = _links.SingleOrDefault(l => l.Id == id);
		if (existingLink is not null)
		{
			_links.Remove(existingLink);
		}

		_links.Add(new() { Id = id, Uri = link.Uri!.ToString() });
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task DeleteLinkAsync(Guid id)
	{
		var link = _links.Single(link => link.Id == id);
		_links.Remove(link);
		return Task.CompletedTask;
	}

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
	public Task PutTransactionAsync(Guid id, TransactionCreationModel transaction)
	{
		var existingTransaction = _transactions.SingleOrDefault(t => t.Id == id);
		if (existingTransaction is not null)
		{
			_transactions.Remove(existingTransaction);
		}

		_transactions.Add(new()
		{
			Id = id,
			BookedAt = transaction.BookedAt,
			ValuedAt = transaction.ValuedAt,
			ReconciledAt = transaction.ReconciledAt,
			Description = transaction.Description,
			CreatedAt = Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
			ModifiedAt = Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
		});

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task<Transaction> GetTransactionAsync(Guid id)
	{
		return Task.FromResult(_transactions.Single(transaction => transaction.Id == id));
	}

	/// <inheritdoc />
	public Task<List<Transaction>> GetTransactionsAsync(Instant? from, Instant? to)
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
	public Task<List<Link>> GetTransactionLinksAsync(Guid transactionId, CancellationToken cancellationToken = default)
	{
		var links = _transactionLinks
			.Where(kvp => kvp.Key == transactionId)
			.Select(kvp => kvp.Value)
			.Select(id => _links.Single(link => link.Id == id))
			.ToList();

		return Task.FromResult(links);
	}

	/// <inheritdoc />
	public Task AddLinkToTransactionAsync(Guid transactionId, Guid linkId)
	{
		_transactionLinks.Add(new(transactionId, linkId));
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task RemoveLinkFromTransactionAsync(Guid transactionId, Guid linkId)
	{
		var relation = _transactionLinks.Single(kvp => kvp.Key == transactionId && kvp.Value == linkId);
		_transactionLinks.Remove(relation);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task<List<Transfer>> GetTransfersAsync(Guid transactionId, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_transfers.Where(transfer => transfer.TransactionId == transactionId).ToList());
	}

	/// <inheritdoc />
	public Task<Transfer> GetTransferAsync(Guid transactionId, Guid id, CancellationToken cancellationToken = default)
	{
		var transfers = _transfers.Where(transfer => transfer.TransactionId == transactionId);
		return Task.FromResult(transfers.Single(transfer => transfer.Id == id));
	}

	/// <inheritdoc />
	public Task PutTransferAsync(Guid transactionId, Guid id, TransferCreation transfer) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task DeleteTransferAsync(Guid transactionId, Guid id) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task<List<Purchase>> GetPurchasesAsync(Guid transactionId, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_purchases.Where(purchase => purchase.TransactionId == transactionId).ToList());
	}

	/// <inheritdoc />
	public Task<Purchase> GetPurchaseAsync(Guid transactionId, Guid id, CancellationToken cancellationToken = default)
	{
		var purchases = _purchases.Where(transfer => transfer.TransactionId == transactionId);
		return Task.FromResult(purchases.Single(purchase => purchase.Id == id));
	}

	/// <inheritdoc />
	public Task PutPurchaseAsync(Guid transactionId, Guid id, PurchaseCreation purchase) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task DeletePurchaseAsync(Guid transactionId, Guid id) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task<Account> GetAccountAsync(Guid id)
	{
		return Task.FromResult(_accounts.Single(account => account.Id == id));
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
	public Task<List<Category>> GetCategoriesAsync()
	{
		return Task.FromResult<List<Category>>(new() { new() { Name = "Foo" }, new() { Name = "Bar" } });
	}

	/// <inheritdoc />
	public Task<Category> GetCategoryAsync(Guid id) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<Guid> CreateCategoryAsync(CategoryCreation category) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task PutCategoryAsync(Guid id, CategoryCreation category) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task DeleteCategoryAsync(Guid id) => throw new NotImplementedException();
}
