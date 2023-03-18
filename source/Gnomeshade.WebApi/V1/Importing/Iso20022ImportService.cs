// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Data;
using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;

using Microsoft.Extensions.Logging;

using NodaTime;

using VMelnalksnis.ISO20022DotNet.MessageSets.BankToCustomerCashManagement.V2.AccountReport;

namespace Gnomeshade.WebApi.V1.Importing;

/// <inheritdoc />
public sealed class Iso20022ImportService : TransactionImportService<ReportEntry2>
{
	/// <summary>Initializes a new instance of the <see cref="Iso20022ImportService"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="clock">A clock that provides access to the current time.</param>
	/// <param name="accountUnitOfWork">Unit of work for managing accounts and all related entities.</param>
	/// <param name="accountRepository">The repository for performing CRUD operations on <see cref="AccountEntity"/>.</param>
	/// <param name="inCurrencyRepository">The repository for performing CRUD operations on <see cref="AccountInCurrencyEntity"/>.</param>
	/// <param name="currencyRepository">The repository for performing CRUD operations on <see cref="CurrencyEntity"/>.</param>
	/// <param name="transactionRepository">The repository for performing CRUD operations on <see cref="TransactionEntity"/>.</param>
	/// <param name="transferRepository">The repository for performing CRUD operations on <see cref="TransferEntity"/>.</param>
	public Iso20022ImportService(
		ILogger<Iso20022ImportService> logger,
		IClock clock,
		AccountUnitOfWork accountUnitOfWork,
		AccountRepository accountRepository,
		AccountInCurrencyRepository inCurrencyRepository,
		CurrencyRepository currencyRepository,
		TransactionRepository transactionRepository,
		TransferRepository transferRepository)
		: base(
			logger,
			clock,
			accountUnitOfWork,
			accountRepository,
			inCurrencyRepository,
			currencyRepository,
			transactionRepository,
			transferRepository)
	{
	}
}
