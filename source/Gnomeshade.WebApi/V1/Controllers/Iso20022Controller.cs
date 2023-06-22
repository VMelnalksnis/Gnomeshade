// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Models.Importing;
using Gnomeshade.WebApi.V1.Authentication;
using Gnomeshade.WebApi.V1.Importing;
using Gnomeshade.WebApi.V1.Importing.Results;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NodaTime;

using VMelnalksnis.ISO20022DotNet.MessageSets.BankToCustomerCashManagement.V2.AccountReport;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>Manages ISO 20022 messages.</summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class Iso20022Controller : ControllerBase
{
	private readonly ILogger<Iso20022Controller> _logger;
	private readonly Iso20022AccountReportReader _reportReader;
	private readonly TransactionRepository _transactionRepository;
	private readonly TransferRepository _transferRepository;
	private readonly TransactionUnitOfWork _transactionUnitOfWork;
	private readonly DbConnection _dbConnection;
	private readonly Mapper _mapper;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
	private readonly Iso20022ImportService _importService;

	/// <summary>Initializes a new instance of the <see cref="Iso20022Controller"/> class.</summary>
	/// <param name="logger">Context specific logger.</param>
	/// <param name="reportReader">Bank account report reader.</param>
	/// <param name="transactionRepository">The repository for performing CRUD operations on <see cref="TransactionEntity"/>.</param>
	/// <param name="transferRepository">The repository for performing CRUD operations on <see cref="TransferEntity"/>.</param>
	/// <param name="transactionUnitOfWork">Unit of work for managing transactions and all related entities.</param>
	/// <param name="dbConnection">Database connection for managing transactions.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="dateTimeZoneProvider">Provider of time zone information.</param>
	/// <param name="importService">Service for importing transactions from external sources.</param>
	public Iso20022Controller(
		ILogger<Iso20022Controller> logger,
		Iso20022AccountReportReader reportReader,
		TransactionRepository transactionRepository,
		TransferRepository transferRepository,
		TransactionUnitOfWork transactionUnitOfWork,
		DbConnection dbConnection,
		Mapper mapper,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Iso20022ImportService importService)
	{
		_logger = logger;
		_reportReader = reportReader;
		_transactionRepository = transactionRepository;
		_transactionUnitOfWork = transactionUnitOfWork;
		_dbConnection = dbConnection;
		_mapper = mapper;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_importService = importService;
		_transferRepository = transferRepository;
	}

	/// <summary>Imports transactions from an ISO 20022 Bank-To-Customer Account Report v02.</summary>
	/// <returns>Summary of all created or references transactions, accounts and products.</returns>
	/// <param name="report">The report to import.</param>
	/// <response code="200">Transactions were successfully imported.</response>
	[HttpPost]
	[ProducesResponseType(Status200OK)]
	public async Task<ActionResult<AccountReportResult>> Import([FromForm] Iso20022Report report)
	{
		var dateTimeZone = _dateTimeZoneProvider.GetZoneOrNull(report.TimeZone);
		if (dateTimeZone is null)
		{
			ModelState.AddModelError(nameof(Iso20022Report.TimeZone), "Unknown timezone");
			return BadRequest(ModelState);
		}

		var user = User.ToApplicationUser();
		_logger.LogDebug("Resolved user {UserId}", user.Id);

		var accountReport = await ReadReport(report.Report);
		_logger.LogDebug("Reading Account Report {ReportId}", accountReport.Identification);

		await using var dbTransaction = await _dbConnection.OpenAndBeginTransaction();

		var (reportAccount, currency, createdAccount) = await _importService.FindUserAccountAsync(
			new(accountReport.Account.Identification.Iban, accountReport.Account.Currency),
			user,
			dbTransaction);

		_logger.LogDebug("Matched report account to {AccountName}", reportAccount.Name);

		var resultBuilder = new AccountReportResultBuilder(_mapper, reportAccount, createdAccount);

		var (bankAccount, bankCreated) = await _importService.FindBankAccountAsync(
			new(accountReport.Account.Servicer?.FinancialInstitutionIdentification.Name, accountReport.Account.Servicer?.FinancialInstitutionIdentification.Bic),
			user,
			currency,
			dbTransaction);

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
						user,
						dateTimeZone);

					if (transaction.Transaction.Id != Guid.Empty)
					{
						return (transaction, false);
					}

					var id = await _transactionUnitOfWork.AddAsync(transaction.Transaction, dbTransaction);
					var transferId =
						await _transferRepository.AddAsync(
							transaction.Transfer with { Id = Guid.NewGuid(), TransactionId = id },
							dbTransaction);
					var t1 = await _transactionRepository.GetByIdAsync(id, user.Id, dbTransaction);
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

		await dbTransaction.CommitAsync();
		return Ok(resultBuilder.ToResult());
	}

	private static Instant GetBookingDate(DateAndDateTimeChoice? dateAndDateTimeChoice, DateTimeZone dateTimeZone)
	{
		if (dateAndDateTimeChoice is null)
		{
			throw new ArgumentNullException(nameof(dateAndDateTimeChoice));
		}

		if (dateAndDateTimeChoice.DateTime is null && dateAndDateTimeChoice.Date is null)
		{
			throw new ArgumentException("Choice does not contain any values", nameof(dateAndDateTimeChoice));
		}

		if (dateAndDateTimeChoice.DateTime is not null)
		{
			var dateTime = dateAndDateTimeChoice.DateTime.Value;
			return dateTime.InZoneStrictly(dateTimeZone).ToInstant();
		}

		var date = dateAndDateTimeChoice.Date!.Value;
		return date.AtStartOfDayInZone(dateTimeZone).ToInstant();
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

	private async Task<(TransactionEntity Transaction, TransferEntity Transfer)> Translate(
		DbTransaction dbTransaction,
		AccountReportResultBuilder resultBuilder,
		ReportEntry2 reportEntry,
		AccountEntity reportAccount,
		AccountEntity bankAccount,
		UserEntity user,
		DateTimeZone dateTimeZone)
	{
		_logger.LogTrace("Parsing transaction {ServicerReference}", reportEntry.AccountServicerReference);

		_logger.LogTrace("Entry contains {EntryDetailCount} details", reportEntry.EntryDetails.Count);
		var entryDetails = reportEntry.EntryDetails.First();
		_logger.LogTrace("Entry details contains {TransactionDetailCount} details", entryDetails.TransactionDetails.Count);
		var transactionDetails = entryDetails.TransactionDetails.First();

		var importableTransaction = new ImportableTransaction(
			reportEntry.AccountServicerReference,
			transactionDetails.References?.Proprietary?.Reference,
			reportEntry.Amount.Value,
			reportEntry.Amount.Currency,
			reportEntry.CreditDebitIndicator,
			GetBookingDate(reportEntry.BookingDate, dateTimeZone),
			null,
			string.Join(string.Empty, transactionDetails.RemittanceInformation?.Unstructured ?? new()),
			reportEntry.AmountDetails?.InstructedAmount is null
				? reportEntry.Amount.Currency
				: reportEntry.AmountDetails.InstructedAmount.Amount.Currency,
			reportEntry.AmountDetails?.InstructedAmount is null
				? reportEntry.Amount.Value
				: reportEntry.AmountDetails.InstructedAmount.Amount.Value,
			transactionDetails.RelatedParties?.CreditorAccount?.Identification.Iban ?? transactionDetails.RelatedParties?.DebtorAccount?.Identification.Iban,
			transactionDetails.RelatedParties?.CreditorAccount?.Name ?? transactionDetails.RelatedParties?.DebtorAccount?.Name,
			reportEntry.BankTransactionCode.Domain?.Code,
			reportEntry.BankTransactionCode.Domain?.Family.Code,
			reportEntry.BankTransactionCode.Domain?.Family.SubFamilyCode);

		return await _importService.Translate(
			dbTransaction,
			resultBuilder,
			importableTransaction,
			reportAccount,
			bankAccount,
			user,
			reportEntry);
	}
}
