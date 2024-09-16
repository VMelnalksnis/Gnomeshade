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
using Gnomeshade.WebApi.Models.Loans;
using Gnomeshade.WebApi.V1;

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V2.Controllers;

/// <summary>CRUD operations on loan payment entity.</summary>
public sealed class LoanPaymentsController(Mapper mapper, LoanPaymentRepository repository, DbConnection dbConnection)
	: CreatableBase<LoanPaymentRepository, LoanPaymentEntity, LoanPayment, LoanPaymentCreation>(mapper, repository, dbConnection)
{
	/// <inheritdoc cref="ILoanClient.GetLoanPaymentAsync"/>
	/// <response code="200">Successfully got the loan payment.</response>
	/// <response code="404">Loan payment with the specified id does not exist.</response>
	[ProducesResponseType<LoanPayment>(Status200OK)]
	public override Task<ActionResult<LoanPayment>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="ILoanClient.GetLoanPaymentsAsync(CancellationToken)"/>
	/// <response code="200">Successfully got all loans payments.</response>
	[ProducesResponseType<LoanPayment>(Status200OK)]
	public override Task<List<LoanPayment>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="ILoanClient.CreateLoanPaymentAsync"/>
	/// <response code="201">A new loan payments was created.</response>
	public override Task<ActionResult> Post(LoanPaymentCreation loanPayment) =>
		base.Post(loanPayment);

	/// <inheritdoc cref="ILoanClient.PutLoanPaymentAsync"/>
	/// <response code="201">A new loan payment was created.</response>
	/// <response code="204">An existing loan payment was replaced.</response>
	public override Task<ActionResult> Put(Guid id, LoanPaymentCreation loanPayment) =>
		base.Put(id, loanPayment);

	// ReSharper disable once RedundantOverriddenMember

	/// <inheritdoc cref="ILoanClient.DeleteLoanPaymentAsync"/>
	/// <response code="204">Loan payment was successfully deleted.</response>
	/// <response code="404">Loan payment with the specified id does not exist.</response>
	/// <response code="409">Loan payment cannot be deleted because some other entity is still referencing it.</response>
	public override Task<ActionResult> Delete(Guid id) =>
		base.Delete(id);

	/// <inheritdoc />
	protected override async Task<ActionResult> UpdateExistingAsync(Guid id, LoanPaymentCreation creation, UserEntity user)
	{
		var payment = Mapper.Map<LoanPaymentEntity>(creation) with
		{
			Id = id,
			ModifiedByUserId = user.Id,
		};

		return await Repository.UpdateAsync(payment) switch
		{
			1 => NoContent(),
			_ => StatusCode(Status403Forbidden),
		};
	}

	/// <inheritdoc />
	protected override async Task<ActionResult> CreateNewAsync(Guid id, LoanPaymentCreation creation, UserEntity user)
	{
		var loan = Mapper.Map<LoanPaymentEntity>(creation) with
		{
			Id = id,
			CreatedByUserId = user.Id,
			ModifiedByUserId = user.Id,
		};

		await Repository.AddAsync(loan);
		return CreatedAtAction(nameof(Get), new { id }, id);
	}
}
