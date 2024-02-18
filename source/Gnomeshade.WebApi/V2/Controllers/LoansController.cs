// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Loans;
using Gnomeshade.WebApi.V1;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V2.Controllers;

/// <summary>CRUD operations on loan entity.</summary>
public sealed class LoansController : CreatableBase<Loan2Repository, Loan2Entity, Loan, LoanCreation>
{
	private readonly LoanPaymentRepository _paymentRepository;

	/// <summary>Initializes a new instance of the <see cref="LoansController"/> class.</summary>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="Loan2Entity"/>.</param>
	/// <param name="paymentRepository">The repository for performing CRUD operations on <see cref="LoanPaymentEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	public LoansController(Mapper mapper, Loan2Repository repository, LoanPaymentRepository paymentRepository, DbConnection dbConnection)
		: base(mapper, repository, dbConnection)
	{
		_paymentRepository = paymentRepository;
	}

	/// <inheritdoc cref="ILoanClient.GetLoanAsync"/>
	/// <response code="200">Successfully got the loan.</response>
	/// <response code="404">Loan with the specified id does not exist.</response>
	[ProducesResponseType<Loan>(Status200OK)]
	public override Task<ActionResult<Loan>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="ILoanClient.GetLoansAsync"/>
	/// <response code="200">Successfully got all loans.</response>
	[ProducesResponseType<List<Loan>>(Status200OK)]
	public override Task<List<Loan>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="ILoanClient.CreateLoanAsync"/>
	/// <response code="201">A new loan was created.</response>
	public override Task<ActionResult> Post(LoanCreation loan) =>
		base.Post(loan);

	/// <inheritdoc cref="ILoanClient.PutLoanAsync"/>
	/// <response code="201">A new loan was created.</response>
	/// <response code="204">An existing loan was replaced.</response>
	public override Task<ActionResult> Put(Guid id, LoanCreation loan) =>
		base.Put(id, loan);

	/// <inheritdoc cref="ILoanClient.DeleteLoanAsync"/>
	/// <response code="204">Loan was successfully deleted.</response>
	/// <response code="404">Loan with the specified id does not exist.</response>
	// ReSharper disable once RedundantOverriddenMember
	public override Task<ActionResult> Delete(Guid id) =>
		base.Delete(id);

	/// <inheritdoc cref="ILoanClient.GetLoanPaymentsAsync(Guid, CancellationToken)"/>
	/// <response code="200">Successfully got all loan payments.</response>
	[HttpGet("{id:guid}/LoanPayments")]
	[ProducesResponseType<List<LoanPayment>>(Status200OK)]
	public async Task<List<LoanPayment>> GetLoanPayments(Guid id, CancellationToken cancellationToken)
	{
		var entities = await _paymentRepository.GetAllForLoanAsync(id, ApplicationUser.Id, cancellationToken);
		return entities.Select(entity => Mapper.Map<LoanPayment>(entity)).ToList();
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(Guid id, LoanCreation creation, UserEntity user)
	{
		var loan = Mapper.Map<Loan2Entity>(creation) with
		{
			Id = id,
			ModifiedByUserId = user.Id,
		};

		await Repository.UpdateAsync(loan);
		return NoContent();
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(Guid id, LoanCreation creation, UserEntity user)
	{
		var loan = Mapper.Map<Loan2Entity>(creation) with
		{
			Id = id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
		};

		await Repository.AddAsync(loan);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}
}
