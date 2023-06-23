// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Client.Results;
using Gnomeshade.WebApi.Models;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Authentication;
using Gnomeshade.WebApi.Models.Importing;
using Gnomeshade.WebApi.Models.Owners;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.DesignTime;

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
	private static readonly List<Category> _categories;
	private static readonly List<Loan> _loans;
	private static readonly List<User> _users;
	private static readonly List<Ownership> _ownerships;
	private static readonly List<Owner> _owners;
	private static readonly List<Access> _accesses;

	static DesignTimeGnomeshadeClient()
	{
		var euro = new Currency { Id = Guid.NewGuid(), Name = "Euro", AlphabeticCode = "EUR" };
		var usd = new Currency { Id = Guid.NewGuid(), Name = "United States Dollar", AlphabeticCode = "USD" };
		_currencies = new() { euro, usd };

		var counterparty = new Counterparty { Id = Guid.Empty, Name = "John Doe" };
		var otherCounterparty = new Counterparty { Id = Guid.NewGuid(), Name = "Jane Doe" };
		_counterparties = new() { counterparty, otherCounterparty };

		var cash = new Account
		{
			Id = Guid.Empty,
			Name = "Cash",
			CounterpartyId = counterparty.Id,
			PreferredCurrency = euro,
			Currencies = new()
			{
				new() { Id = Guid.NewGuid(), CurrencyId = euro.Id, CurrencyAlphabeticCode = euro.AlphabeticCode },
				new() { Id = Guid.NewGuid(), CurrencyId = usd.Id, CurrencyAlphabeticCode = usd.AlphabeticCode },
			},
		};
		var spending = new Account
		{
			Id = Guid.NewGuid(),
			Name = "Spending",
			CounterpartyId = counterparty.Id,
			PreferredCurrency = euro,
			Currencies = new() { new() { Id = Guid.NewGuid(), CurrencyId = euro.Id, CurrencyAlphabeticCode = euro.AlphabeticCode } },
		};
		_accounts = new() { cash, spending };

		var kilogram = new Unit { Id = Guid.NewGuid(), Name = "Kilogram" };
		var gram = new Unit { Id = Guid.NewGuid(), Name = "Gram", ParentUnitId = kilogram.Id, Multiplier = 1000m };
		_units = new() { kilogram, gram };

		var food = new Category { Id = Guid.Empty, Name = "Food" };
		_categories = new() { food };

		var bread = new Product { Id = Guid.NewGuid(), Name = "Bread", CategoryId = food.Id, UnitId = kilogram.Id };
		var milk = new Product { Id = Guid.NewGuid(), Name = "Milk", CategoryId = food.Id };
		_products = new() { bread, milk };

		var transaction = new Transaction
		{
			Id = Guid.Empty,
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
				TargetAccountId = cash.Currencies.First().Id,
				BookedAt = SystemClock.Instance.GetCurrentInstant(),
			},
			new()
			{
				Id = Guid.NewGuid(),
				TransactionId = transaction.Id,
				SourceAmount = 1.95m,
				SourceAccountId = spending.Currencies.Single().Id,
				TargetAmount = 1.95m,
				TargetAccountId = cash.Currencies.First().Id,
				BookedAt = SystemClock.Instance.GetCurrentInstant(),
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

		_loans = new()
		{
			new()
			{
				TransactionId = transaction.Id,
				IssuingCounterpartyId = counterparty.Id,
				ReceivingCounterpartyId = otherCounterparty.Id,
				Amount = 100,
				CurrencyId = euro.Id,
			},
			new()
			{
				TransactionId = transaction.Id,
				IssuingCounterpartyId = counterparty.Id,
				ReceivingCounterpartyId = otherCounterparty.Id,
				Amount = -5,
				CurrencyId = euro.Id,
			},
		};

		var owner = new Access { Id = Guid.NewGuid(), Name = "Owner" };
		var read = new Access { Id = Guid.NewGuid(), Name = "Read" };
		_accesses = new() { owner, read };

		_users = new()
		{
			new() { Id = counterparty.Id },
			new() { Id = otherCounterparty.Id },
		};

		_owners = new()
		{
			new() { Id = counterparty.Id, Name = "Private" },
			new() { Id = otherCounterparty.Id, Name = "Private" },
		};

		_ownerships = new()
		{
			new() { Id = counterparty.Id, OwnerId = counterparty.Id, UserId = counterparty.Id, AccessId = owner.Id },
			new() { Id = otherCounterparty.Id, OwnerId = otherCounterparty.Id, UserId = otherCounterparty.Id, AccessId = owner.Id },
			new() { Id = Guid.NewGuid(), OwnerId = counterparty.Id, UserId = otherCounterparty.Id, AccessId = read.Id },
		};
	}

	/// <inheritdoc />
	public Task<LoginResult> LogInAsync(Login login) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<ExternalLoginResult> SocialRegister() => throw new NotImplementedException();

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

	/// <param name="cancellationToken"></param>
	/// <inheritdoc />
	public Task<Counterparty> GetMyCounterpartyAsync(CancellationToken cancellationToken = default) =>
		Task.FromResult(_counterparties.Single(counterparty => counterparty.Id == Guid.Empty));

	/// <inheritdoc />
	public Task<Counterparty> GetCounterpartyAsync(Guid id, CancellationToken cancellationToken = default) =>
		Task.FromResult(_counterparties.Single(counterparty => counterparty.Id == id));

	/// <param name="cancellationToken"></param>
	/// <inheritdoc />
	public Task<List<Counterparty>> GetCounterpartiesAsync(CancellationToken cancellationToken = default) =>
		Task.FromResult(_counterparties.ToList());

	/// <inheritdoc />
	public Task<Guid> CreateCounterpartyAsync(CounterpartyCreation counterparty) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task PutCounterpartyAsync(Guid id, CounterpartyCreation counterparty) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task MergeCounterpartiesAsync(Guid targetId, Guid sourceId) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<Guid> CreateTransactionAsync(TransactionCreation transaction) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task PutTransactionAsync(Guid id, TransactionCreation transaction)
	{
		var existingTransaction = _transactions.SingleOrDefault(t => t.Id == id);
		if (existingTransaction is not null)
		{
			_transactions.Remove(existingTransaction);
		}

		_transactions.Add(new()
		{
			Id = id,
			ReconciledAt = transaction.ReconciledAt,
			Description = transaction.Description,
			CreatedAt = Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
			ModifiedAt = Instant.FromDateTimeOffset(DateTimeOffset.UtcNow),
		});

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task<Transaction> GetTransactionAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_transactions.Single(transaction => transaction.Id == id));
	}

	/// <inheritdoc />
	public async Task<DetailedTransaction> GetDetailedTransactionAsync(
		Guid id,
		CancellationToken cancellationToken = default)
	{
		var transaction = await GetTransactionAsync(id, cancellationToken);
		var transfers = await GetTransfersAsync(cancellationToken);
		return DetailedTransaction.FromTransaction(transaction) with
		{
			BookedAt = transfers.Select(transfer => transfer.BookedAt).Max(),
			ValuedAt = transfers.Select(transfer => transfer.ValuedAt).Max(),
			Transfers = transfers,
			Purchases = await GetPurchasesAsync(transaction.Id, cancellationToken),
			Loans = await GetLoansAsync(transaction.Id, cancellationToken),
			Links = await GetTransactionLinksAsync(transaction.Id, cancellationToken),
		};
	}

	/// <inheritdoc />
	public Task<List<Transaction>> GetTransactionsAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_transactions.ToList());
	}

	/// <inheritdoc />
	public Task<List<DetailedTransaction>> GetDetailedTransactionsAsync(
		Interval interval,
		CancellationToken cancellationToken = default)
	{
		var transactions = _transactions
			.Select(transaction => GetDetailedTransactionAsync(transaction.Id, cancellationToken))
			.Select(task => task.Result)
			.ToList();

		return Task.FromResult(transactions);
	}

	/// <inheritdoc />
	public Task DeleteTransactionAsync(Guid id)
	{
		_transactions.Remove(_transactions.Single(transaction => transaction.Id == id));
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task MergeTransactionsAsync(Guid targetId, Guid sourceId) =>
		throw new NotImplementedException();

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
	public Task<List<Transfer>> GetTransfersAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_transfers.ToList());
	}

	/// <inheritdoc />
	public Task<Transfer> GetTransferAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_transfers.Single(transfer => transfer.Id == id));
	}

	/// <inheritdoc />
	public Task PutTransferAsync(Guid id, TransferCreation transfer) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task DeleteTransferAsync(Guid id) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task<List<Purchase>> GetPurchasesAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_purchases.ToList());
	}

	/// <inheritdoc />
	public Task<List<Purchase>> GetPurchasesAsync(Guid transactionId, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_purchases.Where(purchase => purchase.TransactionId == transactionId).ToList());
	}

	/// <inheritdoc />
	public Task<Purchase> GetPurchaseAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_purchases.Single(purchase => purchase.Id == id));
	}

	/// <inheritdoc />
	public Task PutPurchaseAsync(Guid id, PurchaseCreation purchase)
	{
		var existingPurchase = _purchases.SingleOrDefault(p => p.Id == id) ?? new Purchase();
		existingPurchase = existingPurchase with
		{
			Id = id,
			TransactionId = purchase.TransactionId!.Value,
			ProductId = purchase.ProductId!.Value,
			Amount = purchase.Amount!.Value,
			Price = purchase.Price!.Value,
			CurrencyId = purchase.CurrencyId!.Value,
			DeliveryDate = purchase.DeliveryDate,
		};
		_purchases.Remove(existingPurchase);
		_purchases.Add(existingPurchase);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task DeletePurchaseAsync(Guid id) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task<List<Loan>> GetLoansAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_loans.ToList());
	}

	/// <inheritdoc />
	public Task<List<Loan>> GetLoansAsync(Guid transactionId, CancellationToken cancellationToken = default)
	{
		var loans = _loans.Where(loan => loan.TransactionId == transactionId).ToList();
		return Task.FromResult(loans);
	}

	/// <inheritdoc />
	public Task<List<Loan>> GetCounterpartyLoansAsync(
		Guid counterpartyId,
		CancellationToken cancellationToken = default)
	{
		var loans = _loans
			.Where(loan =>
				loan.IssuingCounterpartyId == counterpartyId ||
				loan.ReceivingCounterpartyId == counterpartyId)
			.ToList();
		return Task.FromResult(loans);
	}

	/// <inheritdoc />
	public Task<Loan> GetLoanAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var loan = _loans.Single(loan => loan.Id == id);
		return Task.FromResult(loan);
	}

	/// <inheritdoc />
	public Task PutLoanAsync(Guid id, LoanCreation loan)
	{
		var existing = _loans.SingleOrDefault(l => l.Id == id);
		if (existing is not null)
		{
			_loans.Remove(existing);
		}

		existing ??= new() { TransactionId = loan.TransactionId!.Value, Id = id };
		existing = existing with
		{
			IssuingCounterpartyId = loan.IssuingCounterpartyId!.Value,
			ReceivingCounterpartyId = loan.ReceivingCounterpartyId!.Value,
			Amount = loan.Amount!.Value,
			CurrencyId = loan.CurrencyId!.Value,
		};

		_loans.Add(existing);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task DeleteLoanAsync(Guid id)
	{
		var loan = _loans.Single(loan => loan.Id == id);
		_loans.Remove(loan);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task<List<Transaction>> GetRelatedTransactionAsync(Guid id, CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task AddRelatedTransactionAsync(Guid id, Guid relatedId) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task RemoveRelatedTransactionAsync(Guid id, Guid relatedId) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task<Account> GetAccountAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_accounts.Single(account => account.Id == id));
	}

	/// <inheritdoc />
	public Task<List<Account>> GetAccountsAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_accounts.ToList());
	}

	/// <inheritdoc />
	public Task<Guid> CreateAccountAsync(AccountCreation account) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task PutAccountAsync(Guid id, AccountCreation account) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<Guid> AddCurrencyToAccountAsync(Guid id, AccountInCurrencyCreation currency) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task RemoveCurrencyFromAccountAsync(Guid id, Guid currencyId) =>
		throw new NotImplementedException();

	/// <param name="cancellationToken"></param>
	/// <inheritdoc />
	public Task<List<Currency>> GetCurrenciesAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_currencies.ToList());
	}

	/// <inheritdoc />
	public Task<List<Balance>> GetAccountBalanceAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var balances = _accounts
			.Single(account => account.Id == id)
			.Currencies
			.Select(c => new Balance
			{
				AccountInCurrencyId = c.Id,
				SourceAmount = _transfers.Where(t => t.SourceAccountId == c.Id).Sum(t => t.SourceAmount),
				TargetAmount = _transfers.Where(t => t.TargetAccountId == c.Id).Sum(t => t.TargetAmount),
			})
			.ToList();

		return Task.FromResult(balances);
	}

	/// <param name="cancellationToken"></param>
	/// <inheritdoc />
	public Task<List<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_products.ToList());
	}

	/// <inheritdoc />
	public Task<Product> GetProductAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_products.Single(product => product.Id == id));
	}

	/// <inheritdoc />
	public Task DeleteProductAsync(Guid id)
	{
		_products.Remove(_products.Single(product => product.Id == id));
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task<Unit> GetUnitAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_units.Single(unit => unit.Id == id));
	}

	/// <param name="cancellationToken"></param>
	/// <inheritdoc />
	public Task<List<Unit>> GetUnitsAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_units.ToList());
	}

	/// <inheritdoc />
	public Task PutProductAsync(Guid id, ProductCreation product)
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
	public Task PutUnitAsync(Guid id, UnitCreation unit)
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
	public Task<List<string>> GetInstitutionsAsync(string countryCode, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(new List<string> { "SANDBOXFINANCE_SFIN0000" });
	}

	/// <inheritdoc />
	public Task<ImportResult> ImportAsync(string id) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task AddPurchasesFromDocument(Guid transactionId, Guid linkId) => throw new NotImplementedException();

	/// <param name="cancellationToken"></param>
	/// <inheritdoc />
	public Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
		Task.FromResult(_categories.ToList());

	/// <inheritdoc />
	public Task<Category> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_categories.Single(category => category.Id == id));
	}

	/// <inheritdoc />
	public Task<Guid> CreateCategoryAsync(CategoryCreation category)
	{
		var id = Guid.NewGuid();
		_categories.Add(new()
		{
			Id = id,
			Name = category.Name,
			Description = category.Description,
			CategoryId = category.CategoryId,
		});

		return Task.FromResult(id);
	}

	/// <inheritdoc />
	public Task PutCategoryAsync(Guid id, CategoryCreation category)
	{
		var existing = _categories.SingleOrDefault(c => c.Id == id);
		if (existing is not null)
		{
			_categories.Remove(existing);
		}

		_categories.Add(new()
		{
			Id = id,
			Name = category.Name,
			Description = category.Description,
			CategoryId = category.CategoryId,
		});

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task DeleteCategoryAsync(Guid id)
	{
		var category = _categories.Single(c => c.Id == id);
		_categories.Remove(category);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task<List<Purchase>> GetProductPurchasesAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var purchases = _purchases.Where(purchase => purchase.ProductId == id).ToList();
		return Task.FromResult(purchases);
	}

	/// <inheritdoc />
	public Task<List<Access>> GetAccessesAsync(CancellationToken cancellationToken = default) =>
		Task.FromResult(_accesses.ToList());

	/// <inheritdoc />
	public Task DeleteOwnerAsync(Guid id) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<List<Ownership>> GetOwnershipsAsync(CancellationToken cancellationToken = default) =>
		Task.FromResult(_ownerships.ToList());

	/// <inheritdoc />
	public Task<List<Owner>> GetOwnersAsync(CancellationToken cancellationToken = default) =>
		Task.FromResult(_owners.ToList());

	/// <inheritdoc />
	public Task PutOwnerAsync(Guid id, OwnerCreation owner)
	{
		var storedOwner = _owners.SingleOrDefault(o => o.Id == id);
		if (storedOwner is null)
		{
			storedOwner = new() { Id = id };
			_owners.Add(storedOwner);
		}

		storedOwner.Name = owner.Name;
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task PutOwnershipAsync(Guid id, OwnershipCreation ownership)
	{
		var storedOwnership = _ownerships.SingleOrDefault(o => o.Id == id);
		if (storedOwnership is null)
		{
			storedOwnership = new() { Id = id };
			_ownerships.Add(storedOwnership);
		}

		storedOwnership.AccessId = ownership.AccessId;
		storedOwnership.OwnerId = ownership.OwnerId ?? throw new NotImplementedException();
		storedOwnership.UserId = ownership.UserId;
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task DeleteOwnershipAsync(Guid id)
	{
		_ownerships.Remove(_ownerships.Single(ownership => ownership.Id == id));
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task<List<User>> GetUsersAsync(CancellationToken cancellationToken = default) =>
		Task.FromResult(_users.ToList());
}
