// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Transactions;
using Gnomeshade.WebApi.V1.Authorization;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>CRUD operations on loan entity.</summary>
public sealed class LoansController : TransactionItemController<LoanRepository, LoanEntity, Loan, LoanCreation>
{
	/// <summary>Initializes a new instance of the <see cref="LoansController"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="LoanEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	/// <param name="transactionRepository">Transaction repository for validation of transactions.</param>
	public LoansController(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		LoanRepository repository,
		DbConnection dbConnection,
		TransactionRepository transactionRepository)
		: base(applicationUserContext, mapper, repository, dbConnection, transactionRepository)
	{
	}

	/// <inheritdoc cref="ITransactionClient.GetLoanAsync"/>
	/// <response code="200">Successfully got the loan.</response>
	/// <response code="404">Loan with the specified id does not exist.</response>
	[ProducesResponseType(typeof(Loan), Status200OK)]
	public override Task<ActionResult<Loan>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="ITransactionClient.GetLoansAsync(CancellationToken)"/>
	/// <response code="200">Successfully got all loans.</response>
	[ProducesResponseType(typeof(List<Loan>), Status200OK)]
	public override Task<List<Loan>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="ITransactionClient.PutLoanAsync"/>
	/// <response code="201">A new loan was created.</response>
	/// <response code="204">An existing loan was replaced.</response>
	/// <response code="404">The specified transaction does not exist.</response>
	public override Task<ActionResult> Put(Guid id, [FromBody] LoanCreation product) =>
		base.Put(id, product);

	/// <inheritdoc cref="ITransactionClient.DeleteLoanAsync"/>
	/// <response code="204">Loan was successfully deleted.</response>
	/// <response code="404">Loan with the specified id does not exist.</response>
	// ReSharper disable once RedundantOverriddenMember
	public override Task<StatusCodeResult> Delete(Guid id) =>
		base.Delete(id);
}
