// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Ardalis.SmartEnum;

using AutoMapper;

using Gnomeshade.Core;
using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Repositories;
using Gnomeshade.Interfaces.WebApi.Models.Importing;
using Gnomeshade.Interfaces.WebApi.V1_0.Importing.Results;
using Gnomeshade.Interfaces.WebApi.V1_0.Importing.TransactionCodes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using VMelnalksnis.ISO20022DotNet.MessageSets.BankToCustomerCashManagement.V2.AccountReport;

using static Gnomeshade.Interfaces.WebApi.V1_0.Importing.TransactionCodes.Family;
using static Gnomeshade.Interfaces.WebApi.V1_0.Importing.TransactionCodes.SubFamily;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Importing
{
	/// <summary>
	/// Manages ISO 20022 messages.
	/// </summary>
	[ApiController]
	[ApiVersion("1.0")]
	[Authorize]
	[Route("api/v{version:apiVersion}/[controller]")]
	[SuppressMessage(
		"ReSharper",
		"AsyncConverter.ConfigureAwaitHighlighting",
		Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
	public sealed class Iso20022Controller : ControllerBase
	{
		private readonly ILogger<Iso20022Controller> _logger;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly UserRepository _userRepository;
		private readonly Iso20022AccountReportReader _reportReader;
		private readonly CurrencyRepository _currencyRepository;
		private readonly AccountRepository _accountRepository;
		private readonly AccountInCurrencyRepository _inCurrencyRepository;
		private readonly TransactionRepository _transactionRepository;
		private readonly ProductRepository _productRepository;
		private readonly TransactionUnitOfWork _transactionUnitOfWork;
		private readonly AccountUnitOfWork _accountUnitOfWork;
		private readonly IDbConnection _dbConnection;
		private readonly Mapper _mapper;

		public Iso20022Controller(
			ILogger<Iso20022Controller> logger,
			UserManager<ApplicationUser> userManager,
			UserRepository userRepository,
			Iso20022AccountReportReader reportReader,
			CurrencyRepository currencyRepository,
			AccountRepository accountRepository,
			AccountInCurrencyRepository inCurrencyRepository,
			TransactionRepository transactionRepository,
			ProductRepository productRepository,
			TransactionUnitOfWork transactionUnitOfWork,
			AccountUnitOfWork accountUnitOfWork,
			IDbConnection dbConnection,
			Mapper mapper)
		{
			_logger = logger;
			_userManager = userManager;
			_userRepository = userRepository;
			_reportReader = reportReader;
			_currencyRepository = currencyRepository;
			_accountRepository = accountRepository;
			_inCurrencyRepository = inCurrencyRepository;
			_transactionRepository = transactionRepository;
			_productRepository = productRepository;
			_transactionUnitOfWork = transactionUnitOfWork;
			_accountUnitOfWork = accountUnitOfWork;
			_dbConnection = dbConnection;
			_mapper = mapper;
		}

		/// <summary>
		/// Imports transactions from an ISO 20022 Bank-To-Customer Account Report v02.
		/// </summary>
		/// <param name="formFile">A CAMT.052.001.02 message.</param>
		/// <returns>Summary of all created or references transactions, accounts and products.</returns>
		/// <response code="200">Transactions were successfully imported.</response>
		[HttpPost]
		[ProducesResponseType(Status200OK)]
		public async Task<ActionResult<AccountReportResult>> Import(IFormFile formFile)
		{
			var user = await GetCurrentUser();
			if (user is null)
			{
				return Unauthorized();
			}

			_logger.LogDebug("Resolved user {UserId}", user.Id);

			var accountReport = await ReadReport(formFile);
			_logger.LogDebug("Reading Account Report {ReportId}", accountReport.Identification);

			if (!_dbConnection.State.HasFlag(ConnectionState.Open))
			{
				_dbConnection.Open();
			}

			using var dbTransaction = _dbConnection.BeginTransaction();

			var (reportAccount, currency, createdAccount) =
				await FindAccount(accountReport.Account, user, dbTransaction);
			_logger.LogDebug("Matched report account to {AccountName}", reportAccount.Name);

			var resultBuilder = new AccountReportResultBuilder(_mapper, reportAccount, createdAccount);

			var (bankAccount, bankCreated) =
				await FindBankAccount(accountReport.Account.Servicer, user, currency, dbTransaction);
			resultBuilder.AddAccount(bankAccount, bankCreated);

			var creditEntriesSummary = accountReport.TransactionsSummary?.TotalCreditEntries;
			if (creditEntriesSummary is not null)
			{
				_logger.LogDebug(
					"Report contains {CreditEntryCount} credit entries with sum {CreditSum}",
					creditEntriesSummary.NumberOfEntries,
					creditEntriesSummary.Sum);
			}

			var debitEntriesSummary = accountReport.TransactionsSummary?.TotalDebitEntries;
			if (debitEntriesSummary is not null)
			{
				_logger.LogDebug(
					"Report contains {DebitEntryCount} debit entries with sum {DebitSum}",
					debitEntriesSummary.NumberOfEntries,
					debitEntriesSummary.Sum);
			}

			try
			{
				var importResults =
					accountReport
						.Entry
						.Select(async entry =>
						{
							var transaction = await Translate(
								dbTransaction,
								resultBuilder,
								entry,
								reportAccount,
								bankAccount,
								user);

							if (transaction.Id != Guid.Empty)
							{
								return (transaction, false);
							}

							var id = await _transactionUnitOfWork.AddAsync(transaction, dbTransaction);
							transaction = await _transactionRepository.GetByIdAsync(id, dbTransaction);
							return (transaction, true);
						})
						.Select(task => task.Result)
						.ToList();

				foreach (var (transaction, created) in importResults)
				{
					resultBuilder.AddTransaction(transaction, created);
				}

				dbTransaction.Commit();
				return Ok(resultBuilder.ToResult());
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Failed to import");
				dbTransaction.Rollback();
				throw;
			}
		}

		private async Task<UserEntity?> GetCurrentUser()
		{
			var identityUser = await _userManager.GetUserAsync(User);
			if (identityUser is null)
			{
				return null;
			}

			return await _userRepository.FindByIdAsync(new(identityUser.Id));
		}

		private async Task<AccountReport11> ReadReport(IFormFile formFile)
		{
			_logger.LogDebug(
				"Reading file {FileName} with disposition {Disposition}",
				formFile.FileName,
				formFile.ContentDisposition);

			await using var fileStream = formFile.OpenReadStream();
			var bankToCustomerAccountReport = _reportReader.ReadReport(fileStream);
			var header = bankToCustomerAccountReport.GroupHeader;
			_logger.LogDebug("Reading Bank-To-Customer report {MessageId}", header.MessageIdentification);

			_logger.LogDebug("Report contains {ReportCount} report(s)", bankToCustomerAccountReport.Reports.Count);
			return bankToCustomerAccountReport.Reports.First();
		}

		private async Task<(AccountEntity Account, CurrencyEntity Currency, bool Created)> FindAccount(
			CashAccount20 cashAccount,
			UserEntity user,
			IDbTransaction dbTransaction)
		{
			var iban = cashAccount.Identification.Iban;
			if (iban is null)
			{
				throw new NotSupportedException(
					$"Unsupported account identification scheme {cashAccount.Identification}");
			}

			var currencyCode = cashAccount.Currency;
			if (string.IsNullOrWhiteSpace(currencyCode))
			{
				throw new NotSupportedException($"Cannot create a new account without a currency");
			}

			var currency = await _currencyRepository.FindByAlphabeticCodeAsync(currencyCode);
			if (currency is null)
			{
				throw new KeyNotFoundException($"Could not find currency by alphabetic code {currencyCode}");
			}

			var account = await _accountRepository.FindByIbanAsync(iban);
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
			account = await _accountRepository.GetByIdAsync(id);
			return (account, currency, true);
		}

		private async Task<(AccountEntity Account, bool Created)> FindBankAccount(
			BranchAndFinancialInstitutionIdentification4? identification,
			UserEntity user,
			CurrencyEntity currency,
			IDbTransaction dbTransaction)
		{
			if (identification is null)
			{
				throw new();
			}

			AccountEntity? bankAccount = null!;

			var institutionIdentification = identification.FinancialInstitutionIdentification;
			var bankName = institutionIdentification.Name?.ToUpperInvariant();
			if (!string.IsNullOrWhiteSpace(bankName))
			{
				_logger.LogTrace("Searching for bank account by name {AccountName}", bankName);
				bankAccount = await _accountRepository.FindByNameAsync(bankName);
				if (bankAccount is not null)
				{
					_logger.LogDebug("Matched bank account to {AccountName}", bankAccount.Name);
					return (bankAccount, false);
				}

				// todo create new account based on name
			}

			var bic = institutionIdentification.Bic?.ToUpperInvariant();
			if (!string.IsNullOrWhiteSpace(bic))
			{
				_logger.LogTrace("Searching for bank account by BIC {Bic}", bic);
				bankAccount = await _accountRepository.FindByBicAsync(bic);
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
					Name = bic,
					NormalizedName = bic,
					Bic = bic,
					Currencies = new() { new() { CurrencyId = currency.Id } },
				};

				var id = await _accountUnitOfWork.AddWithCounterpartyAsync(account, dbTransaction);
				account = await _accountRepository.GetByIdAsync(id, dbTransaction);
				return (account, true);
			}

			throw new KeyNotFoundException($"Could not find account by name {bankAccount}");
		}

		private async Task<TransactionEntity> Translate(
			IDbTransaction dbTransaction,
			AccountReportResultBuilder resultBuilder,
			ReportEntry2 reportEntry,
			AccountEntity reportAccount,
			AccountEntity bankAccount,
			UserEntity user)
		{
			_logger.LogTrace("Parsing transaction {ServicerReference}", reportEntry.AccountServicerReference);

			var importHash = await reportEntry.GetHashAsync();
			var existingTransaction = await _transactionRepository.FindByImportHashAsync(importHash, dbTransaction);
			if (existingTransaction is not null)
			{
				resultBuilder.AddTransaction(existingTransaction, false);
				var items = existingTransaction.Items;
				var inCurrencyIds = items
					.Select(item => item.SourceAccountId)
					.Concat(items.Select(item => item.TargetAccountId))
					.Distinct();

				var accounts = inCurrencyIds
					.Select(async id => await _inCurrencyRepository.GetByIdAsync(id))
					.Select(task => task.Result.AccountId)
					.Select(async id => await _accountRepository.GetByIdAsync(id))
					.Select(task => task.Result);

				foreach (var account in accounts)
				{
					resultBuilder.AddAccount(account, false);
				}

				foreach (var itemProduct in items.Select(item => item.Product))
				{
					resultBuilder.AddProduct(itemProduct, false);
				}

				return existingTransaction;
			}

			var amount = reportEntry.Amount.Value;
			_logger.LogTrace("Report entry amount {Amount}", amount);

			var currencyCode = reportEntry.Amount.Currency;
			_logger.LogTrace("Searching for currency {CurrencyAlphabeticCode}", currencyCode);
			var currency =
				await _currencyRepository.FindByAlphabeticCodeAsync(currencyCode) ??
				throw new InvalidOperationException($"Failed to find currency by alphabetic code {currencyCode}");
			_logger.LogTrace("Found currency {CurrencyId}", currency.Name);

			var reportAccountInCurrency = reportAccount.Currencies.Single(aic => aic.CurrencyId == currency.Id);

			var creditDebit = reportEntry.CreditDebitIndicator;
			_logger.LogTrace("Credit or debit indicator {CreditDebit}", creditDebit);

			var bookingDate =
				reportEntry.BookingDate?.DateTime ??
				reportEntry.BookingDate?.Date!.Value ??
				throw new InvalidOperationException("Booking date is null");
			_logger.LogTrace("Booking date {BookingDate}", bookingDate);

			_logger.LogTrace("Entry contains {EntryDetailCount} details", reportEntry.EntryDetails.Count);
			var entryDetails = reportEntry.EntryDetails.First();
			_logger.LogTrace(
				"Entry details contains {TransactionDetailCount} details",
				entryDetails.TransactionDetails.Count);
			var transactionDetails = entryDetails.TransactionDetails.First();

			var bankReference = transactionDetails.References?.Proprietary?.Reference;
			_logger.LogTrace("Bank reference {BankReference}", bankReference);
			var description =
				string.Join(string.Empty, transactionDetails.RemittanceInformation?.Unstructured ?? new());
			_logger.LogTrace("Item description {ItemDescription}", description);

			_logger.LogTrace("Mapping {Code} to product", reportEntry.BankTransactionCode);

			var productName = GetProductName(reportEntry.BankTransactionCode);
			var product = await _productRepository.FindByNameAsync(productName.ToUpperInvariant());
			if (product is null)
			{
				product = new()
				{
					OwnerId = user.Id,
					CreatedByUserId = user.Id,
					ModifiedByUserId = user.Id,
					Name = productName,
					NormalizedName = productName.ToUpperInvariant(),
				};

				var id = await _productRepository.AddAsync(product, dbTransaction);
				product = await _productRepository.GetByIdAsync(id);
				resultBuilder.AddProduct(product, true);
			}
			else
			{
				resultBuilder.AddProduct(product, false);
			}

			_logger.LogTrace("Mapped to {Product}", product);

			var (otherCurrency, otherAmount) = await GetOtherAmount(transactionDetails, amount, currency);

			_logger.LogTrace("Searching for other account by {RelatedParties}", transactionDetails.RelatedParties);
			var otherAccount = await FindOtherAccount(transactionDetails.RelatedParties);
			_logger.LogTrace("Found other account {OtherAccount}", otherAccount?.Name);
			if (otherAccount is null)
			{
				_logger.LogTrace("Searching for other account by {TransactionCode}", reportEntry.BankTransactionCode);
				otherAccount = FindOtherAccount(reportEntry.BankTransactionCode, bankAccount);
				_logger.LogTrace("Found other account {OtherAccount}", otherAccount?.Name);
			}

			if (otherAccount is null && transactionDetails.RelatedParties is not null)
			{
				var name =
					transactionDetails.RelatedParties.Creditor?.Name ??
					transactionDetails.RelatedParties.Debtor?.Name ??
					throw new InvalidOperationException("Could not determine the name of the other account");

				var iban =
					transactionDetails.RelatedParties.CreditorAccount?.Identification.Iban ??
					transactionDetails.RelatedParties.DebtorAccount?.Identification.Iban;

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

				otherAccount = await _accountRepository.GetByIdAsync(id);
				resultBuilder.AddAccount(otherAccount, true);
			}

			if (otherAccount is null)
			{
				_logger.LogTrace("Failed to find other account.");
				throw new();
			}

			resultBuilder.AddAccount(otherAccount, false);

			var otherAccountCurrency = otherAccount.Currencies.Single(aic => aic.CurrencyId == otherCurrency.Id);

			var transactionItem = new TransactionItemEntity
			{
				OwnerId = user.Id,
				CreatedByUserId = user.Id,
				ModifiedByUserId = user.Id,
				BankReference = bankReference,
				Description = description,
				ProductId = product.Id,
			};

			transactionItem = creditDebit == CreditDebitCode.CRDT
				? transactionItem with
				{
					SourceAmount = otherAmount,
					SourceAccountId = otherAccountCurrency.Id,
					TargetAmount = amount,
					TargetAccountId = reportAccountInCurrency.Id,
				}
				: transactionItem with
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
				Date = bookingDate,
				ImportHash = importHash,
				ImportedAt = DateTimeOffset.Now, // todo at db transaction level
				Description = reportEntry.AccountServicerReference,
				Items = new() { transactionItem },
			};

			return transaction;
		}

		private async Task<AccountEntity?> FindOtherAccount(TransactionParty2? relatedParty)
		{
			if (relatedParty is null)
			{
				return null;
			}

			AccountEntity? otherAccount = null;

			var name =
				relatedParty.Creditor?.Name ??
				relatedParty.Debtor?.Name;
			if (!string.IsNullOrWhiteSpace(name))
			{
				otherAccount = await _accountRepository.FindByNameAsync(name.ToUpperInvariant());
				if (otherAccount is not null)
				{
					return otherAccount;
				}
			}

			var iban =
				relatedParty.CreditorAccount?.Identification.Iban ??
				relatedParty.DebtorAccount?.Identification.Iban;
			if (!string.IsNullOrWhiteSpace(iban))
			{
				otherAccount = await _accountRepository.FindByIbanAsync(iban);
			}

			return otherAccount;
		}

		private AccountEntity? FindOtherAccount(
			BankTransactionCodeStructure4 transactionCode,
			AccountEntity bankAccount)
		{
			if (transactionCode.Domain is null)
			{
				return null;
			}

			var domain = Domain.FromName(transactionCode.Domain.Code, true);
			var familyCodeStructure = transactionCode.Domain.Family;
			var family = Family.FromName(familyCodeStructure.Code, true);
			var subFamily = SubFamily.FromName(familyCodeStructure.SubFamilyCode, true);

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

			if (subFamily.Equals(Fees) ||
				(family.Equals(CreditOperation) && subFamily.Equals(SubFamily.NotAvailable)))
			{
				return bankAccount;
			}

			return null;
		}

		private string GetProductName(BankTransactionCodeStructure4 transactionCode)
		{
			if (transactionCode.Domain is null)
			{
				return transactionCode.Proprietary!.Code;
			}

			var domain = Domain.FromName(transactionCode.Domain.Code, true);
			var family = SmartEnum<Family, int>.FromName(transactionCode.Domain.Family.Code, true);
			var subFamily = SmartEnum<SubFamily, int>.FromName(transactionCode.Domain.Family.SubFamilyCode, true);

			return $"{domain.Name} - Family {family.Name}; Subfamily {subFamily.Name}";
		}

		private async Task<(CurrencyEntity Currency, decimal Amount)> GetOtherAmount(
			EntryTransaction2 transaction,
			decimal amount,
			CurrencyEntity currency)
		{
			var amountDetails = transaction.AmountDetails;
			if (amountDetails is null)
			{
				return (currency, amount);
			}

			var otherCurrencyCode = amountDetails.InstructedAmount?.Amount.Currency ?? string.Empty;
			var otherAmount = amountDetails.InstructedAmount!.Amount.Value;
			var otherCurrency = await _currencyRepository.FindByAlphabeticCodeAsync(otherCurrencyCode);
			_logger.LogTrace(
				"Found other currency {Currency} by code {CurrencyCode}",
				otherCurrency,
				otherCurrencyCode);
			if (otherCurrency is null)
			{
				throw new();
			}

			return (otherCurrency, otherAmount);
		}
	}
}
