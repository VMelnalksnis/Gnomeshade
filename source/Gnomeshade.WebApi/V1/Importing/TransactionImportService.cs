// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.V1.Accounts;
using Gnomeshade.WebApi.V1.Importing.Results;
using Gnomeshade.WebApi.V1.Importing.TransactionCodes;

using Microsoft.Extensions.Logging;

using NodaTime;

using VMelnalksnis.ISO20022DotNet.MessageSets.BankToCustomerCashManagement.V2.AccountReport;

using static Microsoft.Extensions.Logging.LogLevel;

namespace Gnomeshade.WebApi.V1.Importing;

/// <summary>Handles importing of transactions from external sources.</summary>
/// <typeparam name="TTransaction">The original type of the transaction being imported.</typeparam>
public abstract partial class TransactionImportService<TTransaction>
{
	private readonly ILogger<TransactionImportService<TTransaction>> _logger;
	private readonly IClock _clock;
	private readonly AccountUnitOfWork _accountUnitOfWork;
	private readonly AccountRepository _accountRepository;
	private readonly AccountInCurrencyRepository _inCurrencyRepository;
	private readonly CurrencyRepository _currencyRepository;
	private readonly TransactionRepository _transactionRepository;
	private readonly TransferRepository _transferRepository;

	/// <summary>Initializes a new instance of the <see cref="TransactionImportService{TTransaction}"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="clock">A clock that provides access to the current time.</param>
	/// <param name="accountUnitOfWork">Unit of work for managing accounts and all related entities.</param>
	/// <param name="accountRepository">The repository for performing CRUD operations on <see cref="AccountEntity"/>.</param>
	/// <param name="inCurrencyRepository">The repository for performing CRUD operations on <see cref="AccountInCurrencyEntity"/>.</param>
	/// <param name="currencyRepository">The repository for performing CRUD operations on <see cref="CurrencyEntity"/>.</param>
	/// <param name="transactionRepository">The repository for performing CRUD operations on <see cref="TransactionEntity"/>.</param>
	/// <param name="transferRepository">The repository for performing CRUD operations on <see cref="TransferEntity"/>.</param>
	protected TransactionImportService(
		ILogger<TransactionImportService<TTransaction>> logger,
		IClock clock,
		AccountUnitOfWork accountUnitOfWork,
		AccountRepository accountRepository,
		AccountInCurrencyRepository inCurrencyRepository,
		CurrencyRepository currencyRepository,
		TransactionRepository transactionRepository,
		TransferRepository transferRepository)
	{
		_logger = logger;
		_clock = clock;
		_accountUnitOfWork = accountUnitOfWork;
		_accountRepository = accountRepository;
		_inCurrencyRepository = inCurrencyRepository;
		_currencyRepository = currencyRepository;
		_transactionRepository = transactionRepository;
		_transferRepository = transferRepository;
	}

	internal async Task<(AccountEntity Account, bool Created)> FindBankAccountAsync(
		Bank? bank,
		UserEntity user,
		CurrencyEntity currency,
		DbTransaction dbTransaction)
	{
		if (bank is null)
		{
			throw new ArgumentNullException(nameof(bank));
		}

		var bankName = bank.Name;
		if (!string.IsNullOrWhiteSpace(bankName))
		{
			SearchingBankAccountByName(bankName);
			var accountByName = await _accountRepository.FindByNameAsync(bankName, user.Id, dbTransaction);
			if (accountByName is not null)
			{
				MatchedBankAccountByName(bankName, accountByName.Id);
				return (accountByName, false);
			}
		}

		var bankBic = bank.Bic;
		if (string.IsNullOrWhiteSpace(bankBic))
		{
			throw new KeyNotFoundException("Could not find account for bank");
		}

		SearchingBankAccountByBic(bankBic);
		var accountByBic = await _accountRepository.FindByBicAsync(bankBic, user.Id, dbTransaction);
		if (accountByBic is not null)
		{
			MatchedBankAccountByBic(bankBic, accountByBic.Id);
			return (accountByBic, false);
		}

		CreatingNewBankAccount();
		var account = new AccountEntity
		{
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			PreferredCurrencyId = currency.Id,
			Name = bankBic,
			Bic = bankBic,
			Currencies = new() { new() { CurrencyId = currency.Id } },
		};

		var accountId = await _accountUnitOfWork.AddWithCounterpartyAsync(account, dbTransaction);
		account = await _accountRepository.GetByIdAsync(accountId, user.Id, dbTransaction);
		CreatedNewBankAccount(accountId, account.Name);

		return (account, true);
	}

	internal async Task<(AccountEntity Account, CurrencyEntity Currency, bool Created)> FindUserAccountAsync(
		UserAccount userAccount,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		var iban = userAccount.Iban;
		var currencyCode = userAccount.CurrencyCode;
		if (string.IsNullOrWhiteSpace(iban) || string.IsNullOrWhiteSpace(currencyCode))
		{
			throw new ArgumentNullException(nameof(userAccount));
		}

		SearchingCurrencyByAlphabeticCode(currencyCode);
		var currency = await _currencyRepository.FindByAlphabeticCodeAsync(currencyCode);
		if (currency is null)
		{
			throw new KeyNotFoundException("Could not find currency by alphabetic code");
		}

		FoundCurrencyByAlphabeticCode(currencyCode, currency.Id);
		SearchingUserAccountByIban(iban);

		var existingAccount = await _accountRepository.FindByIbanAsync(iban, user.Id, dbTransaction);
		if (existingAccount is not null)
		{
			MatchedUserAccountByIban(iban, existingAccount.Id);
			return (existingAccount, currency, false);
		}

		CreatingNewUserAccount();
		var account = new AccountEntity
		{
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			Name = iban,
			CounterpartyId = user.CounterpartyId,
			PreferredCurrencyId = currency.Id,
			Iban = iban,
			AccountNumber = iban,
			Currencies = new() { new() { CurrencyId = currency.Id } },
		};

		var accountId = await _accountUnitOfWork.AddAsync(account, dbTransaction);
		account = await _accountRepository.GetByIdAsync(accountId, user.Id, dbTransaction);
		CreatedNewUserAccount(accountId, account.Name);

		return (account, currency, true);
	}

	internal async Task<(TransactionEntity Transaction, TransferEntity Transfer)> Translate(
		DbTransaction dbTransaction,
		AccountReportResultBuilder resultBuilder,
		ImportableTransaction importableTransaction,
		AccountEntity reportAccount,
		AccountEntity bankAccount,
		UserEntity user,
		TTransaction originalTransaction)
	{
		var bankReference = importableTransaction.BankReference;
		var externalReference = importableTransaction.ExternalReference;

		if (!string.IsNullOrWhiteSpace(bankReference))
		{
			SearchingTransferByBankReference(bankReference);

			var existingTransfer = await _transferRepository.FindByBankReferenceAsync(bankReference, user.Id, dbTransaction);
			if (existingTransfer is not null)
			{
				FoundTransferByBankReference(bankReference, existingTransfer.Id);
				return await ExistingTransfer(existingTransfer, user, dbTransaction, resultBuilder);
			}

			FoundTransfersByBankReference(bankReference, 0);
		}

		if (!string.IsNullOrWhiteSpace(externalReference))
		{
			SearchingTransferByExternalReference(externalReference);

			var transfers = (await _transferRepository.GetByExternalReferenceAsync(externalReference, user.Id, dbTransaction)).ToArray();
			if (transfers is[var existingTransfer])
			{
				FoundTransferByExternalReference(externalReference, existingTransfer.Id);

				if (existingTransfer.BankReference is not null && existingTransfer.BankReference != bankReference)
				{
					// This is a new transfer which previously was not imported due to duplicate bank reference
				}
				else
				{
					// After moving bank reference to external reference, need to set the bank reference if possible
					if (existingTransfer.BankReference is null && !string.IsNullOrWhiteSpace(bankReference))
					{
						existingTransfer.BankReference = bankReference;
						await _transferRepository.UpdateAsync(existingTransfer, dbTransaction);
					}

					return await ExistingTransfer(existingTransfer, user, dbTransaction, resultBuilder);
				}
			}

			var count = transfers.Length;
			FoundTransfersByExternalReference(externalReference, count);
		}

		var amount = importableTransaction.Amount;
		var currencyCode = importableTransaction.CurrencyCode;
		var currency = await _currencyRepository.FindByAlphabeticCodeAsync(currencyCode);
		if (currency is null)
		{
			throw new KeyNotFoundException("Could not find currency by alphabetic code");
		}

		var reportAccountInCurrency = reportAccount.Currencies.Single(aic => aic.CurrencyId == currency.Id);
		var creditDebit = importableTransaction.CreditDebitCode;
		var bookingDate = importableTransaction.BookingDate;
		var valueDate = importableTransaction.ValueDate;
		var description = importableTransaction.Description;
		var otherAmount = importableTransaction.OtherAmount;
		var otherCurrencyCode = importableTransaction.OtherCurrencyCode;
		var otherCurrency = await _currencyRepository.FindByAlphabeticCodeAsync(otherCurrencyCode);
		if (otherCurrency is null)
		{
			throw new KeyNotFoundException("Could not find currency by alphabetic code");
		}

		var otherAccount = await FindOtherAccountAsync(
			importableTransaction.OtherAccountIban,
			importableTransaction.OtherAccountName,
			user,
			dbTransaction);

		var otherAccountIsBank = IsOtherAccountBank(
				importableTransaction.DomainCode,
				importableTransaction.FamilyCode,
				importableTransaction.SubFamilyCode) ||
			IsOtherAccountBank(originalTransaction);

		if (otherAccount is null && otherAccountIsBank)
		{
			otherAccount = bankAccount;
			resultBuilder.AddAccount(otherAccount, false);
		}

		var otherAccountName = importableTransaction.OtherAccountName;
		if (otherAccount is null && !string.IsNullOrWhiteSpace(otherAccountName))
		{
			var otherAccountIban = importableTransaction.OtherAccountIban;
			var accountToCreate = new AccountEntity
			{
				OwnerId = user.Id,
				CreatedByUserId = user.Id,
				ModifiedByUserId = user.Id,
				PreferredCurrencyId = otherCurrency.Id,
				Name = otherAccountName,
				Iban = otherAccountIban,
				AccountNumber = otherAccountIban,
				Currencies = new() { new() { CurrencyId = otherCurrency.Id } },
			};

			var accountId = await _accountUnitOfWork.AddWithCounterpartyAsync(accountToCreate, dbTransaction);
			otherAccount = await _accountRepository.GetByIdAsync(accountId, user.Id, dbTransaction);
			resultBuilder.AddAccount(otherAccount, true);
		}

		if (otherAccount is null)
		{
			UsingUnidentifiedAccount();
			otherAccount = await _accountRepository.FindByNameAsync(ReservedNames.Unidentified, user.Id, dbTransaction);

			if (otherAccount is null)
			{
				CreatingUnidentifiedAccount();
				var account = new AccountEntity
				{
					Id = Guid.NewGuid(),
					CreatedByUserId = user.Id,
					ModifiedByUserId = user.Id,
					OwnerId = user.Id,
					Name = ReservedNames.Unidentified,
					PreferredCurrencyId = currency.Id,
					Currencies = new() { new() { CurrencyId = currency.Id } },
				};

				var accountId = await _accountUnitOfWork.AddWithCounterpartyAsync(account, dbTransaction);
				CreatedUnidentifiedAccount(accountId);
				otherAccount = await _accountRepository.GetByIdAsync(accountId, user.Id, dbTransaction);
				resultBuilder.AddAccount(otherAccount, true);
			}
		}

		SearchingForCurrencyInAccount(otherCurrency.AlphabeticCode, otherAccount.Id);
		var otherAccountCurrency = otherAccount.Currencies.SingleOrDefault(aic => aic.CurrencyId == otherCurrency.Id);
		if (otherAccountCurrency is null)
		{
			AddingCurrencyToAccount(otherCurrency.AlphabeticCode, otherAccount.Id);
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
			AddedCurrencyToAccount(otherCurrency.AlphabeticCode, otherAccount.Id);
		}

		var transfer = new TransferEntity
		{
			OwnerId = user.Id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
			BankReference = bankReference,
			ExternalReference = externalReference,
			Order = 0,
		};

		transfer = creditDebit == CreditDebitCode.CRDT
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
			ValuedAt = valueDate,
			ImportedAt = _clock.GetCurrentInstant(),
			Description = description,
		};

		return (transaction, transfer);
	}

	/// <summary>Checks whether the other account for <paramref name="transaction"/> is the bank.</summary>
	/// <param name="transaction">The original transaction to check.</param>
	/// <returns><c>true</c> if the counterparty is the bank, otherwise <c>false</c>.</returns>
	protected virtual bool IsOtherAccountBank(TTransaction transaction) => false;

	private async Task<AccountEntity?> FindOtherAccountAsync(
		string? iban,
		string? name,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		if (!string.IsNullOrWhiteSpace(iban))
		{
			var account = await _accountRepository.FindByIbanAsync(iban, user.Id, dbTransaction);
			if (account is not null)
			{
				return account;
			}
		}

		if (!string.IsNullOrWhiteSpace(name))
		{
			var account = await _accountRepository.FindByNameAsync(name.ToUpperInvariant(), user.Id, dbTransaction);
			if (account is not null)
			{
				return null;
			}
		}

		return null;
	}

	private bool IsOtherAccountBank(string? domainCode, string? familyCode, string? subFamilyCode)
	{
		if (string.IsNullOrWhiteSpace(domainCode))
		{
			return false;
		}

		var domain = Domain.FromName(domainCode, true);

		var family = string.IsNullOrWhiteSpace(familyCode)
			? Family.Other
			: Family.FromName(familyCode, true);

		var subFamily = string.IsNullOrWhiteSpace(subFamilyCode)
			? SubFamily.NotAvailable
			: SubFamily.FromName(subFamilyCode, true);

		if (domain.Equals(Domain.LoansAndDeposits))
		{
			return true;
		}

		if (domain.Equals(Domain.AccountManagement) &&
			family.Equals(Family.CreditOperation) &&
			subFamily.Equals(SubFamily.Interest))
		{
			return true;
		}

		if (!domain.Equals(Domain.Payments))
		{
			return false;
		}

		return
			SubFamily.BankSubFamilies.Contains(subFamily) ||
			(family.Equals(Family.CreditOperation) && subFamily.Equals(SubFamily.NotAvailable));
	}

	private async Task<(TransactionEntity Transaction, TransferEntity Transfer)> ExistingTransfer(
		TransferEntity existingTransfer,
		UserEntity user,
		DbTransaction dbTransaction,
		AccountReportResultBuilder resultBuilder)
	{
		var existingTransaction = await _transactionRepository
			.GetByIdAsync(existingTransfer.TransactionId, user.Id, dbTransaction);

		resultBuilder.AddTransaction(existingTransaction, false);
		var inCurrencyIds = new[] { existingTransfer.SourceAccountId, existingTransfer.TargetAccountId }
			.Distinct();

		var accounts = inCurrencyIds
			.Select(async id => await _inCurrencyRepository.GetByIdAsync(id, user.Id))
			.Select(task => task.Result.AccountId)
			.Select(async id => await _accountRepository.GetByIdAsync(id, user.Id))
			.Select(task => task.Result);

		foreach (var account in accounts)
		{
			resultBuilder.AddAccount(account, false);
		}

		return (existingTransaction, existingTransfer);
	}

	[LoggerMessage(EventId = 1000, Level = Debug, Message = "Searching for bank account by name {AccountName}")]
	private partial void SearchingBankAccountByName(string accountName);

	[LoggerMessage(EventId = 1001, Level = Information, Message = "Matched bank account {AccountName} to existing account {AccountId}")]
	private partial void MatchedBankAccountByName(string accountName, Guid accountId);

	[LoggerMessage(EventId = 1002, Level = Debug, Message = "Searching for bank account by BIC {Bic}")]
	private partial void SearchingBankAccountByBic(string bic);

	[LoggerMessage(EventId = 1003, Level = Information, Message = "Matched bank account {Bic} to existing account {AccountId}")]
	private partial void MatchedBankAccountByBic(string bic, Guid accountId);

	[LoggerMessage(EventId = 1004, Level = Debug, Message = "Could not find existing bank account, creating a new one")]
	private partial void CreatingNewBankAccount();

	[LoggerMessage(EventId = 1005, Level = Information, Message = "Created bank account {AccountId} with name {AccountName}")]
	private partial void CreatedNewBankAccount(Guid accountId, string accountName);

	[LoggerMessage(EventId = 1006, Level = Debug, Message = "Searching for currency by alphabetic code {AlphabeticCode}")]
	private partial void SearchingCurrencyByAlphabeticCode(string alphabeticCode);

	[LoggerMessage(EventId = 1007, Level = Information, Message = "Found currency {CurrencyId} by alphabetic code {AlphabeticCode}")]
	private partial void FoundCurrencyByAlphabeticCode(string alphabeticCode, Guid currencyId);

	[LoggerMessage(EventId = 1008, Level = Debug, Message = "Searching for user account by IBAN {Iban}")]
	private partial void SearchingUserAccountByIban(string iban);

	[LoggerMessage(EventId = 1009, Level = Information, Message = "Matched user account {Iban} to existing account {AccountId}")]
	private partial void MatchedUserAccountByIban(string iban, Guid accountId);

	[LoggerMessage(EventId = 1010, Level = Debug, Message = "Could not find existing user account, creating a new one")]
	private partial void CreatingNewUserAccount();

	[LoggerMessage(EventId = 1011, Level = Information, Message = "Created user account {AccountId} with name {AccountName}")]
	private partial void CreatedNewUserAccount(Guid accountId, string accountName);

	[LoggerMessage(EventId = 1012, Level = Debug, Message = "Searching for transfer by {BankReference}")]
	private partial void SearchingTransferByBankReference(string bankReference);

	[LoggerMessage(EventId = 1013, Level = Information, Message = "Found transfer {TransferId} by {BankReference}")]
	private partial void FoundTransferByBankReference(string bankReference, Guid transferId);

	[LoggerMessage(EventId = 1014, Level = Information, Message = "Found {Count} transfers by {BankReference}")]
	private partial void FoundTransfersByBankReference(string bankReference, int count);

	[LoggerMessage(EventId = 1015, Level = Debug, Message = "Searching for transfer by {ExternalReference}")]
	private partial void SearchingTransferByExternalReference(string externalReference);

	[LoggerMessage(EventId = 1016, Level = Information, Message = "Found transfer {TransferId} by {ExternalReference}")]
	private partial void FoundTransferByExternalReference(string externalReference, Guid transferId);

	[LoggerMessage(EventId = 1017, Level = Information, Message = "Found {Count} transfers by {ExternalReference}")]
	private partial void FoundTransfersByExternalReference(string externalReference, int count);

	[LoggerMessage(EventId = 1018, Level = Debug, Message = "Could not any matching account, using unidentified")]
	private partial void UsingUnidentifiedAccount();

	[LoggerMessage(EventId = 1019, Level = Debug, Message = "Could not find unidentified account, creating it")]
	private partial void CreatingUnidentifiedAccount();

	[LoggerMessage(EventId = 1020, Level = Information, Message = "Created unidentified account {AccountId}")]
	private partial void CreatedUnidentifiedAccount(Guid accountId);

	[LoggerMessage(EventId = 1021, Level = Debug, Message = "Searching for currency {AlphabeticCode} in account {AccountId}")]
	private partial void SearchingForCurrencyInAccount(string alphabeticCode, Guid accountId);

	[LoggerMessage(EventId = 1022, Level = Debug, Message = "Adding currency {AlphabeticCode} to account {AccountId}")]
	private partial void AddingCurrencyToAccount(string alphabeticCode, Guid accountId);

	[LoggerMessage(EventId = 1023, Level = Debug, Message = "Added currency {AlphabeticCode} to account {AccountId}")]
	private partial void AddedCurrencyToAccount(string alphabeticCode, Guid accountId);
}

internal sealed record Bank(string? Name, string? Bic);

internal sealed record UserAccount(string? Iban, string? CurrencyCode);

internal sealed record ImportableTransaction(
	string? BankReference,
	string? ExternalReference,
	decimal Amount,
	string CurrencyCode,
	CreditDebitCode CreditDebitCode,
	Instant BookingDate,
	Instant? ValueDate,
	string? Description,
	string OtherCurrencyCode,
	decimal OtherAmount,
	string? OtherAccountIban,
	string? OtherAccountName,
	string? DomainCode,
	string? FamilyCode,
	string? SubFamilyCode);
