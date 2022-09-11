// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.OpenApi;
using Gnomeshade.WebApi.V1.Authorization;
using Gnomeshade.WebApi.V1.Importing.Paperless;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>Integrations with paperless.</summary>
[ApiController]
[AuthorizeApplicationUser]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class PaperlessController : ControllerBase
{
	private readonly ApplicationUserContext _applicationUserContext;
	private readonly TransactionRepository _transactionRepository;
	private readonly IPaperlessService _paperlessService;

	/// <summary>Initializes a new instance of the <see cref="PaperlessController"/> class.</summary>
	/// <param name="transactionRepository">The repository for performing CRUD operations on <see cref="TransactionEntity"/>.</param>
	/// <param name="applicationUserContext">Context for getting the current application user.</param>
	/// <param name="paperlessService">Service for working with paperless documents.</param>
	public PaperlessController(
		TransactionRepository transactionRepository,
		ApplicationUserContext applicationUserContext,
		IPaperlessService paperlessService)
	{
		_transactionRepository = transactionRepository;
		_applicationUserContext = applicationUserContext;
		_paperlessService = paperlessService;
	}

	/// <inheritdoc cref="IImportClient.AddPurchasesFromDocument"/>
	/// <response code="204">Purchases were successfully added.</response>
	/// <response code="404">A link with <paramref name="linkId"/> does not exist or the document it links to does not exist.</response>
	[HttpPost]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> Post(Guid transactionId, Guid linkId)
	{
		var links = await _transactionRepository.GetAllLinksAsync(transactionId, _applicationUserContext.User.Id);
		var link = links.SingleOrDefault(link => link.Id == linkId);
		if (link is null)
		{
			return NotFound();
		}

		if (!_paperlessService.IsPaperlessDocumentUri(link.Uri))
		{
			ModelState.AddModelError(nameof(linkId), "The specified link does not lead to a paperless document");
			return BadRequest(ModelState);
		}

		var document = await _paperlessService.GetPaperlessDocument(link.Uri);
		if (document is null)
		{
			return NotFound();
		}

		await _paperlessService.AddPurchasesToTransaction(_applicationUserContext.User.Id, transactionId, document);
		return NoContent();
	}
}
