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
using Gnomeshade.WebApi.V1.Transactions;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>CRUD operations on loan entity.</summary>
[Obsolete]
public sealed class LoansController : TransactionItemController<LoanRepository, LoanEntity, Loan, LoanCreation>
{
	/// <summary>Initializes a new instance of the <see cref="LoansController"/> class.</summary>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="LoanEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	/// <param name="transactionRepository">Transaction repository for validation of transactions.</param>
	public LoansController(
		Mapper mapper,
		LoanRepository repository,
		DbConnection dbConnection,
		TransactionRepository transactionRepository)
		: base(mapper, repository, dbConnection, transactionRepository)
	{
	}

	/// <summary>Gets the specified loan.</summary>
	/// <param name="id">The id of the loan to get.</param>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The loan with the specified id.</returns>
	/// <response code="200">Successfully got the loan.</response>
	/// <response code="404">Loan with the specified id does not exist.</response>
	[ProducesResponseType<Loan>(Status200OK)]
	public override Task<ActionResult<Loan>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <summary>Gets all loans.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>All loans.</returns>
	/// <response code="200">Successfully got all loans.</response>
	[ProducesResponseType<List<Loan>>(Status200OK)]
	public override Task<List<Loan>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <summary>Creates a new loan or replaces an existing one, if one exists with the specified id.</summary>
	/// <param name="id">The id of the loan.</param>
	/// <param name="loan">The loan to create or replace.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	/// <response code="201">A new loan was created.</response>
	/// <response code="204">An existing loan was replaced.</response>
	/// <response code="404">The specified transaction does not exist.</response>
	public override Task<ActionResult> Put(Guid id, [FromBody] LoanCreation loan) =>
		base.Put(id, loan);

	/// <summary>Deletes the specified loan.</summary>
	/// <param name="id">The id of the loan to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	/// <response code="204">Loan was successfully deleted.</response>
	/// <response code="404">Loan with the specified id does not exist.</response>
	// ReSharper disable once RedundantOverriddenMember
	public override Task<ActionResult> Delete(Guid id) =>
		base.Delete(id);
}
