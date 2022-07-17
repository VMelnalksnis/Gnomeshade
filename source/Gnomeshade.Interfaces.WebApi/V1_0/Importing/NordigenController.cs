// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Importing;
using Gnomeshade.Interfaces.WebApi.V1_0.Accounts;
using Gnomeshade.Interfaces.WebApi.V1_0.Authorization;
using Gnomeshade.Interfaces.WebApi.V1_0.Importing.Results;
using Gnomeshade.Interfaces.WebApi.V1_0.Importing.TransactionCodes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NodaTime;

using VMelnalksnis.ISO20022DotNet.MessageSets.BankToCustomerCashManagement.V2.AccountReport;
using VMelnalksnis.NordigenDotNet;
using VMelnalksnis.NordigenDotNet.Accounts;
using VMelnalksnis.NordigenDotNet.Institutions;

using static Gnomeshade.Interfaces.WebApi.V1_0.Importing.TransactionCodes.Family;
using static Gnomeshade.Interfaces.WebApi.V1_0.Importing.TransactionCodes.SubFamily;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Importing;

/// <summary>Import data from Nordigen.</summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[AuthorizeApplicationUser]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class NordigenController : ControllerBase
{
	private readonly ApplicationUserContext _applicationUserContext;
	private readonly ILogger<NordigenController> _logger;
	private readonly INordigenClient _nordigenClient;
	private readonly IDbConnection _dbConnection;
	private readonly AccountRepository _accountRepository;
	private readonly TransactionRepository _transactionRepository;
	private readonly TransferRepository _transferRepository;
	private readonly CurrencyRepository _currencyRepository;
	private readonly AccountInCurrencyRepository _inCurrencyRepository;
	private readonly TransactionUnitOfWork _transactionUnitOfWork;
	private readonly AccountUnitOfWork _accountUnitOfWork;
	private readonly Mapper _mapper;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
	private readonly IClock _clock;

	/// <summary>Initializes a new instance of the <see cref="NordigenController"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="logger">Context specific logger.</param>
	/// <param name="nordigenClient">Client for making calls to the Nordigen API.</param>
	/// <param name="dbConnection">Database connection for managing transactions.</param>
	/// <param name="accountRepository">The repository for performing CRUD operations on <see cref="AccountEntity"/>.</param>
	/// <param name="transactionRepository">The repository for performing CRUD operations on <see cref="TransactionEntity"/>.</param>
	/// <param name="transferRepository">The repository for performing CRUD operations on <see cref="TransferEntity"/>.</param>
	/// <param name="currencyRepository">The repository for performing CRUD operations on <see cref="CurrencyEntity"/>.</param>
	/// <param name="inCurrencyRepository">The repository for performing CRUD operations on <see cref="AccountInCurrencyEntity"/>.</param>
	/// <param name="transactionUnitOfWork">Unit of work for managing transactions and all related entities.</param>
	/// <param name="accountUnitOfWork">Unit of work for managing accounts and all related entities.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="dateTimeZoneProvider">Provider of time zone information.</param>
	/// <param name="clock">A clock that provides access to the current time.</param>
	public NordigenController(
		ApplicationUserContext applicationUserContext,
		ILogger<NordigenController> logger,
		INordigenClient nordigenClient,
		IDbConnection dbConnection,
		AccountRepository accountRepository,
		TransactionRepository transactionRepository,
		TransferRepository transferRepository,
		CurrencyRepository currencyRepository,
		AccountInCurrencyRepository inCurrencyRepository,
		TransactionUnitOfWork transactionUnitOfWork,
		AccountUnitOfWork accountUnitOfWork,
		Mapper mapper,
		IDateTimeZoneProvider dateTimeZoneProvider,
		IClock clock)
	{
		_applicationUserContext = applicationUserContext;
		_logger = logger;
		_nordigenClient = nordigenClient;
		_dbConnection = dbConnection;
		_accountRepository = accountRepository;
		_transactionRepository = transactionRepository;
		_transferRepository = transferRepository;
		_currencyRepository = currencyRepository;
		_inCurrencyRepository = inCurrencyRepository;
		_transactionUnitOfWork = transactionUnitOfWork;
		_accountUnitOfWork = accountUnitOfWork;
		_mapper = mapper;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_clock = clock;
	}

	/// <inheritdoc cref="IImportClient.GetInstitutionsAsync"/>
	[HttpGet]
	public async Task<List<string>> GetInstitutions(string countryCode, CancellationToken cancellationToken)
	{
		var institutions = await _nordigenClient.Institutions.GetByCountry(countryCode, cancellationToken);
		return institutions.Select(institution => institution.Id).ToList();
	}

	/// <inheritdoc cref="IImportClient.ImportAsync"/>
	[HttpPost("{id}")]
	public async Task<ActionResult> Import(string id, [Required] string timeZone)
	{
		if (_dbConnection.State is not ConnectionState.Open)
		{
			_dbConnection.Open();
		}

		using var dbTransaction = _dbConnection.BeginTransaction(IsolationLevel.ReadUncommitted);
		try
		{
			var dateTimeZone = _dateTimeZoneProvider.GetZoneOrNull(timeZone);
			if (dateTimeZone is null)
			{
				ModelState.AddModelError(nameof(Iso20022Report.TimeZone), "Unknown timezone");
				return BadRequest(ModelState);
			}

			_logger.LogDebug("Getting requisition for {InstitutionId}", id);
			var existing = await _nordigenClient.Requisitions.Get().SingleOrDefaultAsync(r => r.InstitutionId == id);
			if (existing is null)
			{
				_logger.LogDebug("Creating new requisition for {InstitutionId}", id);
				var requisition = await _nordigenClient.Requisitions.Post(new(new("https://gnomeshade.org/"), id));
				return Redirect(requisition.Link.ToString());
			}

			var tasks = existing.Accounts.Select(async accountId => await _nordigenClient.Accounts.Get(accountId));
			var accounts = await Task.WhenAll(tasks);
			var user = _applicationUserContext.User;
			var results = new List<AccountReportResult>();

			var dataTasks = accounts.Select(async account =>
			{
				var details = await _nordigenClient.Accounts.GetDetails(account.Id);
				var transactions = await _nordigenClient.Accounts.GetTransactions(account.Id);
				return (account, details, transactions);
			});

			var data = await Task.WhenAll(dataTasks);
			foreach (var (account, details, transactions) in data)
			{
				var (reportAccount, currency, createdAccount) =
					await FindAccount(account, details, user, dbTransaction);
				_logger.LogDebug("Matched report account to {AccountName}", reportAccount.Name);

				var resultBuilder = new AccountReportResultBuilder(_mapper, reportAccount, createdAccount);

				var institution = await _nordigenClient.Institutions.Get(account.InstitutionId);
				var (bankAccount, bankCreated) = await FindBankAccount(institution, user, currency, dbTransaction);
				resultBuilder.AddAccount(bankAccount, bankCreated);

				var importResults =
					transactions
						.Booked
						.Select(async bookedTransaction =>
						{
							var transaction = await Translate(
								dbTransaction,
								resultBuilder,
								bookedTransaction,
								reportAccount,
								bankAccount,
								user,
								dateTimeZone);

							if (transaction.Transaction.Id != Guid.Empty)
							{
								return (transaction, false);
							}

							var transactionId =
								await _transactionUnitOfWork.AddAsync(transaction.Transaction, dbTransaction);
							var transferId =
								await _transferRepository.AddAsync(
									transaction.Transfer with { Id = Guid.NewGuid(), TransactionId = transactionId },
									dbTransaction);
							var t1 = await _transactionRepository.GetByIdAsync(transactionId, user.Id, dbTransaction);
							var t2 = await _transferRepository.GetByIdAsync(transferId, user.Id, dbTransaction);
							transaction = (t1, t2);
							return (transaction, true);
						})
						.Select(task => task.Result)
						.ToList();

				foreach (var (transaction, created) in importResults)
				{
					resultBuilder.AddTransaction(transaction.Transaction, created);
					resultBuilder.AddTransfer(transaction.Transfer, created);
				}

				results.Add(resultBuilder.ToResult());
			}

			dbTransaction.Commit();
			return Ok(results);
		}
		catch
		{
			dbTransaction.Rollback();
			throw;
		}
	}

	private static Instant GetBookingDate(BookedTransaction bookedTransaction, DateTimeZone dateTimeZone)
	{
		return bookedTransaction.BookingDate.AtStartOfDayInZone(dateTimeZone).ToInstant();
	}

	private async Task<(AccountEntity Account, CurrencyEntity Currency, bool Created)> FindAccount(
		Account cashAccount,
		AccountDetails cashAccountDetails,
		UserEntity user,
		IDbTransaction dbTransaction)
	{
		var iban = cashAccount.Iban;
		if (iban is null)
		{
			throw new NotSupportedException($"Unsupported account identification scheme {cashAccount.Iban}");
		}

		var currencyCode = cashAccountDetails.Currency;
		if (string.IsNullOrWhiteSpace(currencyCode))
		{
			throw new NotSupportedException($"Cannot create a new account without a currency");
		}

		var currency = await _currencyRepository.FindByAlphabeticCodeAsync(currencyCode);
		if (currency is null)
		{
			throw new KeyNotFoundException($"Could not find currency by alphabetic code {currencyCode}");
		}

		var account = await _accountRepository.FindByIbanAsync(iban, user.Id, dbTransaction);
		if (account is not null)
		{
			return (account, currency, false);
		}

		account = new()
		{
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			Name = iban,
			NormalizedName = iban.ToUpperInvariant(),
			CounterpartyId = user.CounterpartyId,
			PreferredCurrencyId = currency.Id,
			Iban = iban,
			AccountNumber = iban,
			Currencies = new() { new() { CurrencyId = currency.Id } },
		};

		var id = await _accountUnitOfWork.AddAsync(account, dbTransaction);
		account = await _accountRepository.GetByIdAsync(id, user.Id, dbTransaction);
		return (account, currency, true);
	}

	private async Task<(AccountEntity Account, bool Created)> FindBankAccount(
		Institution institution,
		UserEntity user,
		CurrencyEntity currency,
		IDbTransaction dbTransaction)
	{
		AccountEntity? bankAccount = null!;

		var bankName = institution.Name.ToUpperInvariant();
		if (!string.IsNullOrWhiteSpace(bankName))
		{
			_logger.LogTrace("Searching for bank account by name {AccountName}", bankName);
			bankAccount = await _accountRepository.FindByNameAsync(bankName, user.Id, dbTransaction);
			if (bankAccount is not null)
			{
				_logger.LogDebug("Matched bank account to {AccountName}", bankAccount.Name);
				return (bankAccount, false);
			}

			// todo create new account based on name
		}

		var bic = institution.Bic.ToUpperInvariant();
		if (!string.IsNullOrWhiteSpace(bic))
		{
			_logger.LogTrace("Searching for bank account by BIC {Bic}", bic);
			bankAccount = await _accountRepository.FindByBicAsync(bic, user.Id, dbTransaction);
			if (bankAccount is not null)
			{
				_logger.LogDebug("Matched bank account to {Bic}", bic);
				return (bankAccount, false);
			}

			var account = new AccountEntity
			{
				OwnerId = user.Id,
				CreatedByUserId = user.Id,
				ModifiedByUserId = user.Id,
				PreferredCurrencyId = currency.Id,
				Name = institution.Name,
				NormalizedName = institution.Name.ToUpperInvariant(),
				Bic = bic,
				Currencies = new() { new() { CurrencyId = currency.Id } },
			};

			var id = await _accountUnitOfWork.AddWithCounterpartyAsync(account, dbTransaction);
			account = await _accountRepository.GetByIdAsync(id, user.Id, dbTransaction);
			return (account, true);
		}

		throw new KeyNotFoundException($"Could not find account by name {bankAccount}");
	}

	private async Task<(TransactionEntity Transaction, TransferEntity Transfer)> Translate(
		IDbTransaction dbTransaction,
		AccountReportResultBuilder resultBuilder,
		BookedTransaction bookedTransaction,
		AccountEntity reportAccount,
		AccountEntity bankAccount,
		UserEntity user,
		DateTimeZone dateTimeZone)
	{
		_logger.LogTrace("Parsing transaction {ServicerReference}", bookedTransaction.TransactionId);

		var bankReference = bookedTransaction.EntryReference;
		_logger.LogTrace("Bank reference {BankReference}", bankReference);

		var existingTransfer = string.IsNullOrWhiteSpace(bankReference)
			? null
			: await _transferRepository.FindByBankReferenceAsync(bankReference, user.Id, dbTransaction);
		if (existingTransfer is not null)
		{
			var existingTransaction = await _transactionRepository.GetByIdAsync(existingTransfer.TransactionId, user.Id, dbTransaction);
			resultBuilder.AddTransaction(existingTransaction, false);
			var inCurrencyIds = new[] { existingTransfer.SourceAccountId, existingTransfer.TargetAccountId }
				.Distinct();

			var accounts = inCurrencyIds
				.Select(async id => await _inCurrencyRepository.GetByIdAsync(id, user.Id, dbTransaction))
				.Select(task => task.Result.AccountId)
				.Select(async id => await _accountRepository.GetByIdAsync(id, user.Id, dbTransaction))
				.Select(task => task.Result);

			foreach (var account in accounts)
			{
				resultBuilder.AddAccount(account, false);
			}

			return (existingTransaction, existingTransfer);
		}

		var amount = Math.Abs(bookedTransaction.TransactionAmount.Amount);
		_logger.LogTrace("Report entry amount {Amount}", amount);

		var currencyCode = bookedTransaction.TransactionAmount.Currency;
		_logger.LogTrace("Searching for currency {CurrencyAlphabeticCode}", currencyCode);
		var currency =
			await _currencyRepository.FindByAlphabeticCodeAsync(currencyCode) ??
			throw new InvalidOperationException($"Failed to find currency by alphabetic code {currencyCode}");
		_logger.LogTrace("Found currency {CurrencyId}", currency.Name);

		var reportAccountInCurrency = reportAccount.Currencies.Single(aic => aic.CurrencyId == currency.Id);

		var creditDebit = bookedTransaction.AdditionalInformation switch
		{
			"PURCHASE" => CreditDebitCode.DBIT,
			"INWARD TRANSFER" => CreditDebitCode.CRDT,
			"INWARD CLEARING PAYMENT" => CreditDebitCode.CRDT,
			"CARD FEE" => CreditDebitCode.DBIT,
			"OUTWARD TRANSFER" => CreditDebitCode.DBIT,
			"OUTWARD INSTANT PAYMENT" => CreditDebitCode.DBIT,
			_ => throw new ArgumentOutOfRangeException(nameof(bookedTransaction.AdditionalInformation), bookedTransaction.AdditionalInformation, string.Empty),
		};

		_logger.LogTrace("Credit or debit indicator {CreditDebit}", creditDebit);

		var bookingDate = GetBookingDate(bookedTransaction, dateTimeZone);
		_logger.LogTrace("Booking date {BookingDate}", bookingDate);

		var description = bookedTransaction.UnstructuredInformation;
		_logger.LogTrace("Item description {ItemDescription}", description);

		_logger.LogTrace("Mapping {Code} to product", bookedTransaction.BankTransactionCode);

		var (otherCurrency, otherAmount) = GetOtherAmount(amount, currency);

		_logger.LogTrace("Searching for other account by {RelatedParties}", bookedTransaction);
		var otherAccount = await FindOtherAccount(bookedTransaction, user, dbTransaction);
		_logger.LogTrace("Found other account {OtherAccount}", otherAccount?.Name);
		if (otherAccount is null)
		{
			_logger.LogTrace("Searching for other account by {TransactionCode}", bookedTransaction);
			otherAccount = FindOtherAccount(bookedTransaction, bankAccount);
			_logger.LogTrace("Found other account {OtherAccount}", otherAccount?.Name);
		}

		if (otherAccount is null && (bookedTransaction.CreditorName is not null || bookedTransaction.DebtorName is not null))
		{
			var name = bookedTransaction.CreditorName ?? bookedTransaction.DebtorName;
			var iban = bookedTransaction.CreditorAccount?.Iban ?? bookedTransaction.DebtorAccount?.Iban;

			if (name is not null)
			{
				otherAccount = new()
				{
					OwnerId = user.Id,
					CreatedByUserId = user.Id,
					ModifiedByUserId = user.Id,
					PreferredCurrencyId = otherCurrency.Id,
					Name = name,
					NormalizedName = name.ToUpperInvariant(),
					Iban = iban,
					AccountNumber = iban,
					Currencies = new() { new() { CurrencyId = otherCurrency.Id } },
				};

				var id = await _accountUnitOfWork.AddWithCounterpartyAsync(otherAccount, dbTransaction);

				otherAccount = await _accountRepository.GetByIdAsync(id, user.Id, dbTransaction);
				resultBuilder.AddAccount(otherAccount, true);
			}
		}

		if (otherAccount is null && new[] { "CARD FEE" }.Contains(bookedTransaction.BankTransactionCode))
		{
			otherAccount = bankAccount;
			resultBuilder.AddAccount(otherAccount, false);
		}

		if (otherAccount is null)
		{
			_logger.LogTrace("Failed to find other account, using unidentified");
			otherAccount = await _accountRepository.FindByNameAsync(
				ReservedNames.Unidentified.ToUpperInvariant(),
				user.Id,
				dbTransaction);

			if (otherAccount is null)
			{
				var account = new AccountEntity
				{
					Id = Guid.NewGuid(),
					CreatedByUserId = user.Id,
					ModifiedByUserId = user.Id,
					OwnerId = user.Id,
					Name = ReservedNames.Unidentified,
					NormalizedName = ReservedNames.Unidentified.ToUpperInvariant(),
					PreferredCurrencyId = currency.Id,
					Currencies = new() { new() { CurrencyId = currency.Id } },
				};

				await _accountUnitOfWork.AddWithCounterpartyAsync(account, dbTransaction);
				otherAccount = await _accountRepository.FindByNameAsync(
					ReservedNames.Unidentified.ToUpperInvariant(),
					user.Id,
					dbTransaction);
			}
		}

		if (otherAccount is null)
		{
			throw new ApplicationException($"Failed to find account by reserved name {ReservedNames.Unidentified}");
		}

		resultBuilder.AddAccount(otherAccount, false);

		_logger.LogInformation("Searching for currency {OtherCurrency} in {Account}", otherCurrency.AlphabeticCode, otherAccount.Name);
		var otherAccountCurrency = otherAccount.Currencies.SingleOrDefault(aic => aic.CurrencyId == otherCurrency.Id);
		if (otherAccountCurrency is null)
		{
			otherAccountCurrency = new()
			{
				AccountId = otherAccount.Id,
				CreatedByUserId = user.Id,
				CurrencyId = otherCurrency.Id,
				Id = Guid.NewGuid(),
				OwnerId = user.Id,
				ModifiedByUserId = user.Id,
			};
			var otherId = await _inCurrencyRepository.AddAsync(otherAccountCurrency, dbTransaction);
			otherAccountCurrency = await _inCurrencyRepository.GetByIdAsync(otherId, user.Id, dbTransaction);
		}

		var transfer = new TransferEntity
		{
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			BankReference = bankReference,
		};

		transfer = creditDebit is CreditDebitCode.CRDT
			? transfer with
			{
				SourceAmount = otherAmount,
				SourceAccountId = otherAccountCurrency.Id,
				TargetAmount = amount,
				TargetAccountId = reportAccountInCurrency.Id,
			}
			: transfer with
			{
				SourceAmount = amount,
				SourceAccountId = reportAccountInCurrency.Id,
				TargetAmount = otherAmount,
				TargetAccountId = otherAccountCurrency.Id,
			};

		var transaction = new TransactionEntity
		{
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			BookedAt = bookingDate,
			Description = description,
			ImportedAt = _clock.GetCurrentInstant(),
		};

		return (transaction, transfer);
	}

	private async Task<AccountEntity?> FindOtherAccount(
		BookedTransaction bookedTransaction,
		UserEntity user,
		IDbTransaction dbTransaction)
	{
		var iban =
			bookedTransaction.CreditorAccount?.Iban ??
			bookedTransaction.DebtorAccount?.Iban;
		if (!string.IsNullOrWhiteSpace(iban) && await _accountRepository.FindByIbanAsync(iban, user.Id, dbTransaction) is { } ibanAccount)
		{
			return ibanAccount;
		}

		var name =
			bookedTransaction.CreditorName ??
			bookedTransaction.DebtorName;
		if (!string.IsNullOrWhiteSpace(name) && await _accountRepository.FindByNameAsync(name.ToUpperInvariant(), user.Id, dbTransaction) is { } nameAccount)
		{
			return nameAccount;
		}

		return null;
	}

	private AccountEntity? FindOtherAccount(
		BookedTransaction bookedTransaction,
		AccountEntity bankAccount)
	{
		if (bookedTransaction.BankTransactionCode is null)
		{
			return null;
		}

		var codes = bookedTransaction.BankTransactionCode.Split('-');

		var domain = Domain.FromName(codes[0], true);
		var family = Family.FromName(codes[1], true);
		var subFamily = SubFamily.FromName(codes[2], true);

		if (domain.Equals(Domain.Extended))
		{
			return bankAccount;
		}

		if (domain.Equals(Domain.AccountManagement) && family.Equals(CreditOperation) && subFamily.Equals(Interest))
		{
			return bankAccount;
		}

		if (!domain.Equals(Domain.Payments))
		{
			return null;
		}

		if (subFamily.Equals(Fees) || (family.Equals(CreditOperation) && subFamily.Equals(SubFamily.NotAvailable)))
		{
			return bankAccount;
		}

		return null;
	}

	private (CurrencyEntity Currency, decimal Amount) GetOtherAmount(decimal amount, CurrencyEntity currency)
	{
		// Nordigen currently does not return currency information
		return (currency, amount);
	}
}
