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

/// <summary>CRUD operations on purchase entity.</summary>
public sealed class PurchasesController(Mapper mapper, PurchaseRepository repository, DbConnection dbConnection, TransactionRepository transactionRepository)
	: TransactionItemController<PurchaseRepository, PurchaseEntity, Purchase, PurchaseCreation>(mapper, repository, dbConnection, transactionRepository)
{
	/// <inheritdoc cref="ITransactionClient.GetPurchaseAsync"/>
	/// <response code="200">Successfully got the purchase.</response>
	/// <response code="404">Purchase with the specified id does not exist.</response>
	[ProducesResponseType<Purchase>(Status200OK)]
	public override Task<ActionResult<Purchase>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="ITransactionClient.GetPurchasesAsync(CancellationToken)"/>
	/// <response code="200">Successfully got all purchases.</response>
	[ProducesResponseType<List<Purchase>>(Status200OK)]
	public override Task<List<Purchase>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="ITransactionClient.PutPurchaseAsync"/>
	/// <response code="201">A new purchase was created.</response>
	/// <response code="204">An existing purchase was replaced.</response>
	/// <response code="404">The specified purchase does not exist.</response>
	public override Task<ActionResult> Put(Guid id, [FromBody] PurchaseCreation product) =>
		base.Put(id, product);

	// ReSharper disable once RedundantOverriddenMember

	/// <inheritdoc cref="ITransactionClient.DeletePurchaseAsync"/>
	/// <response code="204">Purchase was successfully deleted.</response>
	/// <response code="404">Purchase with the specified id does not exist.</response>
	/// <response code="409">Purchase cannot be deleted because some other entity is still referencing it.</response>
	public override Task<ActionResult> Delete(Guid id) =>
		base.Delete(id);
}
