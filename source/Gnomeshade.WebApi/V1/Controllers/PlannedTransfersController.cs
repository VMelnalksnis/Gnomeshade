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

using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>CRUD operations on planned transfer entity.</summary>
[Route("api/v{version:apiVersion}/Transfers/Planned")]
public sealed class PlannedTransfersController(Mapper mapper, PlannedTransferRepository repository, DbConnection dbConnection, TransactionRepository transactionRepository)
	: TransactionItemController<PlannedTransferRepository, PlannedTransferEntity, PlannedTransfer, PlannedTransferCreation>(mapper, repository, dbConnection, transactionRepository)
{
	/// <inheritdoc cref="ITransactionClient.GetPlannedTransfer"/>
	/// <response code="200">Successfully got the planned transfer.</response>
	/// <response code="404">Planned transfer with the specified id does not exist.</response>
	[ProducesResponseType<PlannedTransfer>(Status200OK)]
	public override Task<ActionResult<PlannedTransfer>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="ITransactionClient.GetPlannedTransfers(CancellationToken)"/>
	/// <response code="200">Successfully got all planned transfers.</response>
	[ProducesResponseType<List<PlannedTransfer>>(Status200OK)]
	public override Task<List<PlannedTransfer>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="ITransactionClient.PutPlannedTransfer"/>
	/// <response code="201">A new planned transfer was created.</response>
	/// <response code="204">An existing planned transfer was replaced.</response>
	/// <response code="404">The specified planned transfer does not exist.</response>
	public override Task<ActionResult> Put(Guid id, [FromBody] PlannedTransferCreation product) =>
		base.Put(id, product);

	// ReSharper disable once RedundantOverriddenMember

	/// <inheritdoc cref="ITransactionClient.DeletePlannedTransfer"/>
	/// <response code="204">Planned transfer was successfully deleted.</response>
	/// <response code="404">Planned transfer with the specified id does not exist.</response>
	/// <response code="409">Planned transfer cannot be deleted because some other entity is still referencing it.</response>
	public override Task<ActionResult> Delete(Guid id) =>
		base.Delete(id);
}
