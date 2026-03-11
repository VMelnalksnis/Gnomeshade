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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>CRUD operations on planned transaction entity.</summary>
[Route("api/v{version:apiVersion}/Transactions/Planned")]
public sealed class PlannedTransactionsController(Mapper mapper, PlannedTransactionRepository repository, DbConnection dbConnection)
	: CreatableBase<PlannedTransactionRepository, PlannedTransactionEntity, PlannedTransaction, PlannedTransactionCreation>(mapper, repository, dbConnection)
{
	/// <inheritdoc cref="ITransactionClient.GetPlannedTransaction"/>
	/// <response code="200">Successfully got the planned transaction.</response>
	/// <response code="404">Planned transaction with the specified id does not exist.</response>
	[ProducesResponseType<PlannedTransaction>(StatusCodes.Status200OK)]
	public override Task<ActionResult<PlannedTransaction>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="ITransactionClient.GetPlannedTransactions(CancellationToken)"/>
	/// <response code="200">Successfully got all planned transactions.</response>
	[ProducesResponseType<List<PlannedTransaction>>(StatusCodes.Status200OK)]
	public override Task<List<PlannedTransaction>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="ITransactionClient.CreatePlannedTransaction"/>
	/// <response code="201">A new planned transaction was created.</response>
	/// <response code="409">A planned transaction with the specified name already exists.</response>
	public override Task<ActionResult> Post(PlannedTransactionCreation transaction) =>
		base.Post(transaction);

	/// <inheritdoc cref="ITransactionClient.PutPlannedTransaction"/>
	/// <response code="201">A new planned transaction was created.</response>
	/// <response code="204">An existing planned transaction was replaced.</response>
	/// <response code="409">A planned transaction with the specified name already exists.</response>
	public override Task<ActionResult> Put(Guid id, PlannedTransactionCreation transaction) =>
		base.Put(id, transaction);

	// ReSharper disable once RedundantOverriddenMember

	/// <inheritdoc cref="ITransactionClient.DeletePlannedTransaction"/>
	/// <response code="204">Planned transaction was successfully deleted.</response>
	/// <response code="404">Planned transaction with the specified id does not exist.</response>
	/// <response code="409">Planned transaction cannot be deleted because some other entity is still referencing it.</response>
	public override Task<ActionResult> Delete(Guid id) =>
		base.Delete(id);

	/// <inheritdoc />
	protected override Task<ActionResult> UpdateExistingAsync(
		Guid id,
		PlannedTransactionCreation creation,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	protected override Task<ActionResult> CreateNewAsync(
		Guid id,
		PlannedTransactionCreation creation,
		UserEntity user,
		DbTransaction dbTransaction)
	{
		throw new NotImplementedException();
	}
}
