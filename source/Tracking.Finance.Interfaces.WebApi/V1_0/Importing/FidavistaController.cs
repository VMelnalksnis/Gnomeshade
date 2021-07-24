// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Tracking.Finance.Data;
using Tracking.Finance.Data.Identity;
using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;
using Tracking.Finance.Imports.Fidavista;

using Account = Tracking.Finance.Data.Models.Account;
using Transaction = Tracking.Finance.Imports.Fidavista.Transaction;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Importing
{
	[ApiController]
	[ApiVersion("1.0")]
	[Authorize]
	[Route("api/v{version:apiVersion}/[controller]")]
	[SuppressMessage(
		"ReSharper",
		"AsyncConverter.ConfigureAwaitHighlighting",
		Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
	public sealed class FidavistaController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly UserRepository _userRepository;
		private readonly FidavistaReader _fidavistaReader;
		private readonly CurrencyRepository _currencyRepository;
		private readonly AccountRepository _accountRepository;
		private readonly TransactionRepository _transactionRepository;
		private readonly ProductRepository _productRepository;
		private readonly TransactionUnitOfWork _transactionUnitOfWork;
		private readonly AccountUnitOfWork _accountUnitOfWork;

		public FidavistaController(
			UserManager<ApplicationUser> userManager,
			UserRepository userRepository,
			FidavistaReader fidavistaReader,
			CurrencyRepository currencyRepository,
			AccountRepository accountRepository,
			TransactionRepository transactionRepository,
			ProductRepository productRepository,
			TransactionUnitOfWork transactionUnitOfWork,
			AccountUnitOfWork accountUnitOfWork)
		{
			_fidavistaReader = fidavistaReader;
			_currencyRepository = currencyRepository;
			_accountRepository = accountRepository;
			_transactionRepository = transactionRepository;
			_productRepository = productRepository;
			_transactionUnitOfWork = transactionUnitOfWork;
			_accountUnitOfWork = accountUnitOfWork;
			_userManager = userManager;
			_userRepository = userRepository;
		}

		[HttpPost]
		public async Task<ActionResult<List<TransactionImportResult>>> Import(IFormFile formFile)
		{
			var user = await GetCurrentUser();
			await using var fileStream = formFile.OpenReadStream();
			var fidavistaDocument = _fidavistaReader.Read(fileStream);

			var statements = fidavistaDocument.Statements;
			if (statements is null || statements.Length != 1)
			{
				return BadRequest("Must have one statement");
			}

			var statement = statements.Single();
			var accounts = statement.Accounts;
			if (accounts is null || accounts.Length != 1)
			{
				return BadRequest("Must have one account within statement");
			}

			var account = accounts.Single();
			var accountEntity = await _accountRepository.FindByIbanAsync(account.AccountNumber);
			if (accountEntity is null)
			{
				throw new();
			}

			var currencyStatements = account.Statements;
			if (currencyStatements is null || currencyStatements.Length != 1)
			{
				return BadRequest("Must have one currency within account");
			}

			var currencyStatement = currencyStatements.Single();
			var currency = await _currencyRepository.FindByAlphabeticCodeAsync(currencyStatement.Currency);
			if (currency is null)
			{
				return BadRequest($"Currency {currencyStatement.Currency} does not exist");
			}

			if (accountEntity.Currencies.All(inCurrency => inCurrency.Currency != currency))
			{
				return BadRequest($"Account {accountEntity.Name} does not have currency {currency.AlphabeticCode}");
			}

			var transactions = currencyStatement.Transactions;
			if (transactions is null || transactions.Length < 1)
			{
				return BadRequest("Currency statements must have at least one transaction");
			}

			var importResults =
				transactions
					.Select(transaction => GetTransactionIdAsync(transaction, user, currency, accountEntity)
						.GetAwaiter().GetResult())
					.ToList();

			return Ok(importResults);
		}

		private async Task<User?> GetCurrentUser()
		{
			var identityUser = await _userManager.GetUserAsync(User);
			if (identityUser is null)
			{
				return null;
			}

			return await _userRepository.FindByIdAsync(new(identityUser.Id));
		}

		private async Task<TransactionImportResult> GetTransactionIdAsync(
			Transaction transaction,
			User user,
			Currency currency,
			Account account)
		{
			var importHash = await transaction.GetHashAsync();
			var existingTransaction = await _transactionRepository.FindByImportHashAsync(importHash);
			if (existingTransaction is not null)
			{
				return new(false, existingTransaction.Id);
			}

			var counterparty = transaction.Counterparties?.SingleOrDefault();
			var counterpartyCurrency = counterparty?.Currency is null
				? currency
				: await _currencyRepository.FindByAlphabeticCodeAsync(counterparty.Currency);

			if (counterpartyCurrency is null)
			{
				throw new();
				// return BadRequest("Failed to find counterparty currency");
			}

			var counterpartyAccount = counterparty is null
				? await _accountRepository.FindByNameAsync("CITADELE")
				: await _accountRepository.FindByIbanAsync(counterparty.AccountNumber);

			if (counterpartyAccount is null && counterparty is not null)
			{
				var newAccount = new Account
				{
					OwnerId = user.Id,
					CreatedByUserId = user.Id,
					ModifiedByUserId = user.Id,
					Name = counterparty.AccountHolder.Name,
					NormalizedName = counterparty.AccountHolder.Name.ToUpperInvariant(),
					Bic = counterparty.BankCode,
					Iban = counterparty.AccountNumber,
					AccountNumber = counterparty.AccountNumber,
				};

				var accountId = await _accountUnitOfWork.AddAsync(newAccount, counterpartyCurrency);
				counterpartyAccount = await _accountRepository.GetByIdAsync(accountId);
			}

			if (counterpartyAccount is null)
			{
				throw new();
				// return BadRequest("Failed to find counterparty account");
			}

			var product = await _productRepository.FindByNameAsync(transaction.TypeName.ToUpperInvariant());
			if (product is null)
			{
				product = new()
				{
					OwnerId = user.Id,
					CreatedByUserId = user.Id,
					ModifiedByUserId = user.Id,
					Name = transaction.TypeName,
					NormalizedName = transaction.TypeName.ToUpperInvariant(),
					Description = "Generated during import",
				};
				var productId = await _productRepository.AddAsync(product);
				product = await _productRepository.GetByIdAsync(productId);
			}

			var transactionEntity = new Data.Models.Transaction
			{
				OwnerId = user.Id,
				CreatedByUserId = user.Id,
				ModifiedByUserId = user.Id,
				Date = transaction.BookDate,
				Description = transaction.PaymentInfo,
				ImportHash = await transaction.GetHashAsync(),
				ImportedAt = DateTimeOffset.Now,
			};

			var credit = transaction.CreditOrDebit == "C";
			var sameCurrency = currency == counterpartyCurrency;

			var userAmount = transaction.AccountAmount;
			var userAccountId = account.Currencies.Single(inCurrency => inCurrency.Currency == currency).Id;
			var counterAmount = sameCurrency ? transaction.AccountAmount : counterparty.Amount.Value;
			var counterAccountId = counterpartyAccount.Currencies
				.Single(inCurrency => inCurrency.Currency == counterpartyCurrency).Id;
			var transactionItem = new TransactionItem
			{
				OwnerId = user.Id,
				CreatedByUserId = user.Id,
				ModifiedByUserId = user.Id,
				BankReference = transaction.BankReference,
				ExternalReference = transaction.DocumentNumber,
				Description = transaction.PaymentInfo,
				ProductId = product.Id,
				Amount = default,
			};

			transactionItem = credit
				? transactionItem with
				{
					SourceAmount = counterAmount,
					SourceAccountId = counterAccountId,
					TargetAmount = userAmount,
					TargetAccountId = userAccountId,
				}
				: transactionItem with
				{
					SourceAmount = userAmount,
					SourceAccountId = userAccountId,
					TargetAmount = counterAmount,
					TargetAccountId = counterAccountId,
				};

			var id = await _transactionUnitOfWork.AddAsync(transactionEntity, new[] { transactionItem });
			return new(true, id);
		}
	}
}
