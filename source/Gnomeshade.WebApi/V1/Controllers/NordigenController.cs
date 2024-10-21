// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Asp.Versioning;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Logging;
using Gnomeshade.WebApi.Models.Importing;
using Gnomeshade.WebApi.V1.Authentication;
using Gnomeshade.WebApi.V1.Importing;
using Gnomeshade.WebApi.V1.Importing.Results;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NodaTime;

using VMelnalksnis.NordigenDotNet;
using VMelnalksnis.NordigenDotNet.Accounts;
using VMelnalksnis.NordigenDotNet.Requisitions;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>Import data from Nordigen.</summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class NordigenController : ControllerBase
{
	private readonly ILogger<NordigenController> _logger;
	private readonly INordigenClient _nordigenClient;
	private readonly DbConnection _dbConnection;
	private readonly TransactionRepository _transactionRepository;
	private readonly TransferRepository _transferRepository;
	private readonly TransactionUnitOfWork _transactionUnitOfWork;
	private readonly Mapper _mapper;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
	private readonly NordigenImportService _importService;

	/// <summary>Initializes a new instance of the <see cref="NordigenController"/> class.</summary>
	/// <param name="logger">Context specific logger.</param>
	/// <param name="nordigenClient">Client for making calls to the Nordigen API.</param>
	/// <param name="dbConnection">Database connection for managing transactions.</param>
	/// <param name="transactionRepository">The repository for performing CRUD operations on <see cref="TransactionEntity"/>.</param>
	/// <param name="transferRepository">The repository for performing CRUD operations on <see cref="TransferEntity"/>.</param>
	/// <param name="transactionUnitOfWork">Unit of work for managing transactions and all related entities.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="dateTimeZoneProvider">Provider of time zone information.</param>
	/// <param name="importService">Service for importing transactions from external sources.</param>
	public NordigenController(
		ILogger<NordigenController> logger,
		INordigenClient nordigenClient,
		DbConnection dbConnection,
		TransactionRepository transactionRepository,
		TransferRepository transferRepository,
		TransactionUnitOfWork transactionUnitOfWork,
		Mapper mapper,
		IDateTimeZoneProvider dateTimeZoneProvider,
		NordigenImportService importService)
	{
		_logger = logger;
		_nordigenClient = nordigenClient;
		_dbConnection = dbConnection;
		_transactionRepository = transactionRepository;
		_transferRepository = transferRepository;
		_transactionUnitOfWork = transactionUnitOfWork;
		_mapper = mapper;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_importService = importService;
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
	[ProducesResponseType<List<AccountReportResult>>(Status200OK)]
	[ProducesResponseType(Status302Found)]
	public async Task<ActionResult> Import(string id, [Required] string timeZone)
	{
		await using var dbTransaction = await _dbConnection.OpenAndBeginTransaction();
		var dateTimeZone = _dateTimeZoneProvider.GetZoneOrNull(timeZone);
		if (dateTimeZone is null)
		{
			ModelState.AddModelError(nameof(Iso20022Report.TimeZone), "Unknown timezone");
			return BadRequest(ModelState);
		}

		_logger.GettingRequisition(id);
		var existing = await _nordigenClient
			.Requisitions
			.Get()
			.OrderByDescending(requisition => requisition.Created)
			.FirstOrDefaultAsync(requisition =>
				requisition.InstitutionId == id &&
				requisition.Status is RequisitionStatus.Ln);

		if (existing is null)
		{
			_logger.CreatingRequisition(id);
			var requisition = await _nordigenClient.Requisitions.Post(new(new("https://gnomeshade.org/"), id));
			return Redirect(requisition.Link.AbsoluteUri);
		}

		var tasks = existing.Accounts.Select(async accountId => await _nordigenClient.Accounts.Get(accountId));
		var accounts = await Task.WhenAll(tasks);
		var user = User.ToApplicationUser();
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
			var (reportAccount, currency, createdAccount) = await _importService
				.FindUserAccountAsync(new(account.Iban, details.Currency), user, dbTransaction);

			_logger.MatchedReportAccount(reportAccount.Name);

			var resultBuilder = new AccountReportResultBuilder(_mapper, reportAccount, createdAccount);

			var institution = await _nordigenClient.Institutions.Get(account.InstitutionId);
			var (bankAccount, bankCreated) = await _importService
				.FindBankAccountAsync(new(institution.Name, institution.Bic), user, currency, dbTransaction);

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

		await dbTransaction.CommitAsync();
		return Ok(results);
	}

	private async Task<(TransactionEntity Transaction, TransferEntity Transfer)> Translate(
		DbTransaction dbTransaction,
		AccountReportResultBuilder resultBuilder,
		BookedTransaction bookedTransaction,
		AccountEntity reportAccount,
		AccountEntity bankAccount,
		UserEntity user,
		DateTimeZone dateTimeZone)
	{
		_logger.ParsingTransaction(bookedTransaction);

		var amount = Math.Abs(bookedTransaction.TransactionAmount.Amount);
		var (domainCode, familyCode, subFamilyCode) = bookedTransaction.GetCode();

		var importableTransaction = new ImportableTransaction(
			bookedTransaction.TransactionId,
			bookedTransaction.EntryReference,
			amount,
			bookedTransaction.TransactionAmount.Currency,
			bookedTransaction.GetCreditDebitCode(),
			bookedTransaction.BookingDate!.Value.AtStartOfDayInZone(dateTimeZone).ToInstant(),
			bookedTransaction.ValueDate?.AtStartOfDayInZone(dateTimeZone).ToInstant(),
			bookedTransaction.UnstructuredInformation,
			bookedTransaction.TransactionAmount.Currency,
			amount,
			bookedTransaction.CreditorAccount?.Iban ?? bookedTransaction.DebtorAccount?.Iban,
			bookedTransaction.CreditorName ?? bookedTransaction.DebtorName,
			domainCode,
			familyCode,
			subFamilyCode);

		return await _importService.Translate(
			dbTransaction,
			resultBuilder,
			importableTransaction,
			reportAccount,
			bankAccount,
			user,
			bookedTransaction);
	}
}
