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
using Gnomeshade.WebApi.Models.Loans;
using Gnomeshade.WebApi.Models.Owners;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Projects;
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
	private static readonly List<User> _users;
	private static readonly List<Ownership> _ownerships;
	private static readonly List<Owner> _owners;
	private static readonly List<Access> _accesses;
	private static readonly List<Loan> _loans;
	private static readonly List<LoanPayment> _loanPayments;
	private static readonly List<Project> _projects;

	private static readonly List<TransactionSchedule> _transactionSchedules;
	private static readonly List<PlannedTransaction> _plannedTransactions;
	private static readonly List<PlannedTransfer> _plannedTransfers;
	private static readonly List<PlannedPurchase> _plannedPurchases;
	private static readonly List<PlannedLoanPayment> _plannedLoanPayments;

	static DesignTimeGnomeshadeClient()
	{
		var euro = new Currency { Id = Guid.NewGuid(), Name = "Euro", AlphabeticCode = "EUR" };
		var usd = new Currency { Id = Guid.NewGuid(), Name = "United States Dollar", AlphabeticCode = "USD" };
		_currencies = [euro, usd];

		var counterparty = new Counterparty { Id = Guid.Empty, Name = "John Doe" };
		var otherCounterparty = new Counterparty { Id = Guid.NewGuid(), Name = "Jane Doe" };
		var bankCounterparty = new Counterparty { Id = Guid.NewGuid(), Name = "Bank" };
		_counterparties = [counterparty, otherCounterparty, bankCounterparty];

		var cash = new Account
		{
			Id = Guid.Empty,
			Name = "Cash",
			CounterpartyId = counterparty.Id,
			PreferredCurrencyId = euro.Id,
			Currencies =
			[
				new() { Id = Guid.NewGuid(), CurrencyId = euro.Id, CurrencyAlphabeticCode = euro.AlphabeticCode },
				new() { Id = Guid.NewGuid(), CurrencyId = usd.Id, CurrencyAlphabeticCode = usd.AlphabeticCode }
			],
		};

		var spending = new Account
		{
			Id = Guid.NewGuid(),
			Name = "Spending",
			CounterpartyId = counterparty.Id,
			PreferredCurrencyId = euro.Id,
			Currencies = [new() { Id = Guid.NewGuid(), CurrencyId = euro.Id, CurrencyAlphabeticCode = euro.AlphabeticCode }],
		};

		var bankAccount = new Account
		{
			Id = Guid.NewGuid(),
			Name = "Bank",
			CounterpartyId = bankCounterparty.Id,
			PreferredCurrencyId = euro.Id,
			Currencies = [new() { Id = Guid.NewGuid(), CurrencyId = euro.Id, CurrencyAlphabeticCode = euro.AlphabeticCode }],
		};

		_accounts = [cash, spending, bankAccount];

		var kilogram = new Unit { Id = Guid.NewGuid(), Name = "Kilogram" };
		var gram = new Unit { Id = Guid.NewGuid(), Name = "Gram", ParentUnitId = kilogram.Id, Multiplier = 1000m };
		_units = [kilogram, gram];

		var food = new Category { Id = Guid.Empty, Name = "Food" };
		var liabilities = new Category { Id = Guid.NewGuid(), Name = "Liabilities" };
		_categories = [food, liabilities];

		var bread = new Product { Id = Guid.NewGuid(), Name = "Bread", CategoryId = food.Id, UnitId = kilogram.Id };
		var milk = new Product { Id = Guid.NewGuid(), Name = "Milk", CategoryId = food.Id };
		var loan = new Product { Id = Guid.NewGuid(), Name = "Loan", CategoryId = liabilities.Id };
		_products = [bread, milk, loan];

		var transaction = new Transaction
		{
			Id = Guid.Empty,
			Description = "Some transaction description",
		};

		_transactions = [transaction];
		_transfers =
		[
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
			}

		];

		_purchases =
		[
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
			}

		];

		_links =
		[
			new() { Id = Guid.Empty, Uri = "https://localhost/documents/1" },
			new() { Id = Guid.NewGuid(), Uri = "https://localhost/documents/1" }
		];

		_transactionLinks =
		[
			new(Guid.Empty, Guid.Empty),
			new(Guid.Empty, _links.Last().Id)
		];

		var owner = new Access { Id = Guid.NewGuid(), Name = "Owner" };
		var read = new Access { Id = Guid.NewGuid(), Name = "Read" };
		_accesses = [owner, read];

		_users =
		[
			new() { Id = counterparty.Id },
			new() { Id = otherCounterparty.Id }
		];

		_owners =
		[
			new() { Id = counterparty.Id, Name = "Private" },
			new() { Id = otherCounterparty.Id, Name = "Private" }
		];

		_ownerships =
		[
			new() { Id = counterparty.Id, OwnerId = counterparty.Id, UserId = counterparty.Id, AccessId = owner.Id },
			new() { Id = otherCounterparty.Id, OwnerId = otherCounterparty.Id, UserId = otherCounterparty.Id, AccessId = owner.Id },
			new() { Id = Guid.NewGuid(), OwnerId = counterparty.Id, UserId = otherCounterparty.Id, AccessId = read.Id }
		];

		_loans =
		[
			new()
			{
				Id = Guid.Empty,
				Name = "Mortgage",
				IssuingCounterpartyId = otherCounterparty.Id,
				ReceivingCounterpartyId = counterparty.Id,
				Principal = 100_000,
				CurrencyId = euro.Id,
			}
		];

		_loanPayments =
		[
			new()
			{
				Id = Guid.Empty,
				LoanId = Guid.Empty,
				TransactionId = Guid.Empty,
				Amount = 500m,
				Interest = 150m,
			}
		];

		_projects =
		[
			new()
			{
				Id = Guid.Empty,
				Name = "Home improvement",
			},
		];

		var transactionSchedule = new TransactionSchedule
		{
			Id = Guid.Empty,
			Name = "Monthly",
			StartingAt = SystemClock.Instance.GetCurrentInstant() + Duration.FromDays(15),
			Period = Period.FromMonths(1),
			Count = 12,
		};

		_transactionSchedules = [transactionSchedule];
		_plannedTransactions = [];
		_plannedTransfers = [];
		_plannedPurchases = [];
		_plannedLoanPayments = [];

		var timeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
		for (var i = 0; i < transactionSchedule.Count; i++)
		{
			var startingAt = transactionSchedule.StartingAt.InZone(timeZone).LocalDateTime;
			for (var j = 0; j < i; j++)
			{
				startingAt += transactionSchedule.Period;
			}

			var bookedAt = startingAt.InZoneStrictly(timeZone).ToInstant();

			var plannedTransaction = new PlannedTransaction
			{
				Id = i is 0 ? Guid.Empty : Guid.NewGuid(),
				ScheduleId = transactionSchedule.Id,
			};

			var plannedPrincipalTransfer = new PlannedTransfer
			{
				Id = i is 0 ? Guid.Empty : Guid.NewGuid(),
				TransactionId = plannedTransaction.Id,
				SourceAmount = 500,
				SourceAccountId = spending.Currencies.Single(account => account.CurrencyId == euro.Id).Id,
				TargetAmount = 500,
				TargetCounterpartyId = bankCounterparty.Id,
				TargetCurrencyId = euro.Id,
				BookedAt = bookedAt,
				Order = 1,
			};

			var plannedInterestTransfer = new PlannedTransfer
			{
				Id = Guid.NewGuid(),
				TransactionId = plannedTransaction.Id,
				SourceAmount = 150,
				SourceAccountId = spending.Currencies.Single(account => account.CurrencyId == euro.Id).Id,
				TargetAmount = 150,
				TargetCounterpartyId = bankCounterparty.Id,
				TargetCurrencyId = euro.Id,
				BookedAt = bookedAt,
				Order = 2,
			};

			var plannedPurchase = new PlannedPurchase
			{
				Id = i is 0 ? Guid.Empty : Guid.NewGuid(),
				TransactionId = plannedTransaction.Id,
				Price = plannedPrincipalTransfer.SourceAmount + plannedInterestTransfer.SourceAmount,
				CurrencyId = euro.Id,
				ProductId = loan.Id,
				Amount = 1,
				ProjectIds = [],
			};

			var plannedLoanPayment = new PlannedLoanPayment
			{
				Id = i is 0 ? Guid.Empty : Guid.NewGuid(),
				LoanId = _loans.Single().Id,
				TransactionId = plannedTransaction.Id,
				Amount = plannedPrincipalTransfer.SourceAmount,
				Interest = plannedInterestTransfer.SourceAmount,
			};

			_plannedTransactions.Add(plannedTransaction);
			_plannedTransfers.AddRange([plannedPrincipalTransfer, plannedInterestTransfer]);
			_plannedPurchases.Add(plannedPurchase);
			_plannedLoanPayments.Add(plannedLoanPayment);
		}
	}

	/// <inheritdoc />
	public Task<LoginResult> LogInAsync(Login login) => Task.FromResult<LoginResult>(new SuccessfulLogin());

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
			LoanPayments = await GetLoanPaymentsForTransactionAsync(transaction.Id, cancellationToken),
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
		MergeTransactionsAsync(targetId, [sourceId]);

	/// <inheritdoc />
	public Task MergeTransactionsAsync(Guid targetId, IEnumerable<Guid> sourceIds) =>
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

		// Performance is not an issue here
		// ReSharper disable once UsageOfDefaultStructEquality
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
	public async Task<Guid> CreatePurchaseAsync(PurchaseCreation purchase)
	{
		var id = Guid.NewGuid();
		await PutPurchaseAsync(id, purchase);
		return id;
	}

	/// <inheritdoc />
	public Task DeletePurchaseAsync(Guid id) =>
		throw new NotImplementedException();

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
	public Task<List<TransactionSchedule>> GetTransactionSchedules(CancellationToken cancellationToken = default) =>
		Task.FromResult(_transactionSchedules.ToList());

	/// <inheritdoc />
	public Task<TransactionSchedule> GetTransactionSchedule(Guid id, CancellationToken cancellationToken = default) =>
		Task.FromResult(_transactionSchedules.Single(schedule => schedule.Id == id));

	/// <inheritdoc />
	public async Task<Guid> CreateTransactionSchedule(TransactionScheduleCreation schedule)
	{
		var id = Guid.NewGuid();
		await PutTransactionSchedule(id, schedule);
		return id;
	}

	/// <inheritdoc />
	public Task PutTransactionSchedule(Guid id, TransactionScheduleCreation schedule)
	{
		var existing = _transactionSchedules.SingleOrDefault(transactionSchedule => transactionSchedule.Id == id);
		existing ??= new();

		existing.Name = schedule.Name;
		existing.StartingAt = schedule.StartingAt;
		existing.Period = schedule.Period;
		existing.Count = schedule.Count;

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public async Task DeleteTransactionSchedule(Guid id)
	{
		var schedule = await GetTransactionSchedule(id);
		_transactionSchedules.Remove(schedule);
	}

	/// <inheritdoc />
	public Task<List<PlannedTransaction>> GetPlannedTransactions(Interval interval, CancellationToken cancellationToken = default)
	{
		var transactions = _plannedTransactions
			.Where(transaction =>
			{
				var date = _plannedTransfers
					.Where(transfer => transfer.TransactionId == transaction.Id)
					.Select(transfer => transfer.BookedAt)
					.Max();

				return date is { } instant && interval.Contains(instant);
			})
			.ToList();

		return Task.FromResult(transactions);
	}

	/// <inheritdoc />
	public Task<List<PlannedTransaction>> GetPlannedTransactions(CancellationToken cancellationToken = default) =>
		Task.FromResult(_plannedTransactions.ToList());

	/// <inheritdoc />
	public Task<List<PlannedTransaction>> GetPlannedTransactions(
		Guid scheduleId,
		CancellationToken cancellationToken = default) =>
		Task.FromResult(_plannedTransactions.Where(transaction => transaction.ScheduleId == scheduleId).ToList());

	/// <inheritdoc />
	public Task<PlannedTransaction> GetPlannedTransaction(Guid id, CancellationToken cancellationToken = default) =>
		Task.FromResult(_plannedTransactions.Single(transaction => transaction.Id == id));

	/// <inheritdoc />
	public Task<Guid> CreatePlannedTransaction(PlannedTransactionCreation transaction) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task PutPlannedTransaction(Guid id, PlannedTransactionCreation transaction) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task DeletePlannedTransaction(Guid id) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<List<PlannedTransfer>> GetPlannedTransfers(CancellationToken cancellationToken = default) =>
		Task.FromResult(_plannedTransfers.ToList());

	/// <inheritdoc />
	public Task<List<PlannedTransfer>> GetPlannedTransfers(Guid transactionId, CancellationToken cancellationToken = default) =>
		Task.FromResult(_plannedTransfers.Where(transfer => transfer.TransactionId == transactionId).ToList());

	/// <inheritdoc />
	public Task<PlannedTransfer> GetPlannedTransfer(Guid id, CancellationToken cancellationToken = default) =>
		Task.FromResult(_plannedTransfers.Single(transfer => transfer.Id == id));

	/// <inheritdoc />
	public Task<Guid> CreatePlannedTransfer(PlannedTransferCreation transfer) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task PutPlannedTransfer(Guid id, PlannedTransferCreation transfer) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task DeletePlannedTransfer(Guid id) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<List<PlannedPurchase>> GetPlannedPurchases(CancellationToken cancellationToken = default) =>
		Task.FromResult(_plannedPurchases.ToList());

	/// <inheritdoc />
	public Task<List<PlannedPurchase>> GetPlannedPurchases(Guid transactionId, CancellationToken cancellationToken = default) =>
		Task.FromResult(_plannedPurchases.Where(transfer => transfer.TransactionId == transactionId).ToList());

	/// <inheritdoc />
	public Task<PlannedPurchase> GetPlannedPurchase(Guid id, CancellationToken cancellationToken = default) =>
		Task.FromResult(_plannedPurchases.Single(purchase => purchase.Id == id));

	/// <inheritdoc />
	public Task<Guid> CreatePlannedPurchase(PlannedPurchaseCreation purchase) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task PutPlannedPurchase(Guid id, PlannedPurchaseCreation purchase) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task DeletePlannedPurchase(Guid id) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<List<PlannedLoanPayment>> GetPlannedLoanPayments(CancellationToken cancellationToken = default) =>
		Task.FromResult(_plannedLoanPayments.ToList());

	/// <inheritdoc />
	public Task<List<PlannedLoanPayment>> GetPlannedLoanPayments(Guid transactionId, CancellationToken cancellationToken = default) =>
		Task.FromResult(_plannedLoanPayments.Where(transfer => transfer.TransactionId == transactionId).ToList());

	/// <inheritdoc />
	public Task<PlannedLoanPayment> GetPlannedLoanPayment(Guid id, CancellationToken cancellationToken = default) =>
		Task.FromResult(_plannedLoanPayments.Single(payment => payment.Id == id));

	/// <inheritdoc />
	public Task<Guid> CreatePlannedLoanPayment(LoanPaymentCreation loanPayment) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task PutPlannedLoanPayment(Guid id, LoanPaymentCreation transfer) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task DeletePlannedLoanPayment(Guid id) => throw new NotImplementedException();

	/// <inheritdoc />
	[Obsolete]
	public Task<List<LegacyLoan>> GetLegacyLoans(CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	[Obsolete]
	public Task DeleteLegacyLoan(Guid id) => throw new NotImplementedException();

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
	public async Task<Guid> CreateProductAsync(ProductCreation product)
	{
		var id = Guid.NewGuid();
		await PutProductAsync(id, product);
		return id;
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

	/// <inheritdoc />
	public Task<Loan> GetLoanAsync(Guid id, CancellationToken cancellationToken = default) =>
		Task.FromResult(_loans.Single(loan => loan.Id == id));

	/// <inheritdoc />
	public Task<List<Loan>> GetLoansAsync(CancellationToken cancellationToken = default) =>
		Task.FromResult(_loans.ToList());

	/// <inheritdoc />
	public Task<Guid> CreateLoanAsync(LoanCreation loan) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task PutLoanAsync(Guid id, LoanCreation loan) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task DeleteLoanAsync(Guid id) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<LoanPayment> GetLoanPaymentAsync(Guid id, CancellationToken cancellationToken = default) =>
		Task.FromResult(_loanPayments.Single(payment => payment.Id == id));

	/// <inheritdoc />
	public Task<List<LoanPayment>> GetLoanPaymentsAsync(CancellationToken cancellationToken = default) =>
		Task.FromResult(_loanPayments.ToList());

	/// <inheritdoc />
	public Task<List<LoanPayment>> GetLoanPaymentsAsync(Guid id, CancellationToken cancellationToken = default) =>
		Task.FromResult(_loanPayments.Where(payment => payment.LoanId == id).ToList());

	/// <inheritdoc />
	public Task<List<LoanPayment>> GetLoanPaymentsForTransactionAsync(Guid id, CancellationToken cancellationToken = default) =>
		Task.FromResult(_loanPayments.Where(payment => payment.LoanId == id).ToList());

	/// <inheritdoc />
	public Task<Guid> CreateLoanPaymentAsync(LoanPaymentCreation loanPayment) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task PutLoanPaymentAsync(Guid id, LoanPaymentCreation loanPayment) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task DeleteLoanPaymentAsync(Guid id) => throw new NotImplementedException();

	/// <inheritdoc />
	public Task<List<Project>> GetProjectsAsync(CancellationToken cancellationToken = default) =>
		Task.FromResult(_projects.ToList());

	/// <inheritdoc />
	public Task<Project> GetProjectAsync(Guid id, CancellationToken cancellationToken = default) =>
		Task.FromResult(_projects.Single(project => project.Id == id));

	/// <inheritdoc />
	public Task<Guid> CreateProjectAsync(ProjectCreation project)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public Task PutProjectAsync(Guid id, ProjectCreation project)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public Task DeleteProjectAsync(Guid id)
	{
		_projects.Remove(_projects.Single(project => project.Id == id));
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task<List<Purchase>> GetProjectPurchasesAsync(Guid id, CancellationToken cancellationToken = default) =>
		Task.FromResult(id == Guid.Empty ? _purchases : []);

	/// <inheritdoc />
	public Task AddPurchaseToProjectAsync(Guid id, Guid purchaseId) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public Task RemovePurchaseFromProjectAsync(Guid id, Guid purchaseId) =>
		throw new NotImplementedException();
}
