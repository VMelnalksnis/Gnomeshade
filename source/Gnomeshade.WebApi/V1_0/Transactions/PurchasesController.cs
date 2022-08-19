// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Transactions;
using Gnomeshade.WebApi.V1_0.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1_0.Transactions;

/// <summary>CRUD operations on purchase entity.</summary>
public sealed class PurchasesController : TransactionItemController<PurchaseRepository, PurchaseEntity, Purchase, PurchaseCreation>
{
	/// <summary>Initializes a new instance of the <see cref="PurchasesController"/> class.</summary>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="mapper">Repository entity and API model mapper.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="repository">The repository for performing CRUD operations on <see cref="PurchaseEntity"/>.</param>
	/// <param name="dbConnection">Database connection for transaction management.</param>
	/// <param name="transactionRepository">Transaction repository for validation of transactions.</param>
	public PurchasesController(
		ApplicationUserContext applicationUserContext,
		Mapper mapper,
		ILogger<PurchasesController> logger,
		PurchaseRepository repository,
		IDbConnection dbConnection,
		TransactionRepository transactionRepository)
		: base(applicationUserContext, mapper, logger, repository, dbConnection, transactionRepository)
	{
	}

	/// <inheritdoc cref="ITransactionClient.GetPurchaseAsync"/>
	/// <response code="200">Successfully got the purchase.</response>
	/// <response code="404">Purchase with the specified id does not exist.</response>
	[ProducesResponseType(typeof(Purchase), Status200OK)]
	public override Task<ActionResult<Purchase>> Get(Guid id, CancellationToken cancellationToken) =>
		base.Get(id, cancellationToken);

	/// <inheritdoc cref="ITransactionClient.GetPurchasesAsync(CancellationToken)"/>
	/// <response code="200">Successfully got all purchases.</response>
	[HttpGet]
	[ProducesResponseType(typeof(List<Purchase>), Status200OK)]
	public override Task<List<Purchase>> Get(CancellationToken cancellationToken) =>
		base.Get(cancellationToken);

	/// <inheritdoc cref="ITransactionClient.PutPurchaseAsync"/>
	/// <response code="201">A new purchase was created.</response>
	/// <response code="204">An existing purchase was replaced.</response>
	/// <response code="404">The specified transaction does not exist.</response>
	public override Task<ActionResult> Put(Guid id, [FromBody] PurchaseCreation product) =>
		base.Put(id, product);

	/// <inheritdoc cref="ITransactionClient.DeletePurchaseAsync"/>
	/// <response code="204">Purchase was successfully deleted.</response>
	/// <response code="404">Purchase with the specified id does not exist.</response>
	// ReSharper disable once RedundantOverriddenMember
	public override Task<StatusCodeResult> Delete(Guid id) =>
		base.Delete(id);
}
