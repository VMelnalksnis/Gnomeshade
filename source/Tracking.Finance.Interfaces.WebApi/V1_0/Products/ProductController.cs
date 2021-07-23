// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

using Tracking.Finance.Data.Identity;
using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Products
{
	/// <summary>
	/// CRUD operations on account entity.
	/// </summary>
	[SuppressMessage(
		"ReSharper",
		"AsyncConverter.ConfigureAwaitHighlighting",
		Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
	public sealed class ProductController : FinanceControllerBase<Product, ProductModel>
	{
		private readonly IDbConnection _dbConnection;
		private readonly ProductRepository _repository;
		private readonly ILogger<ProductController> _logger;

		public ProductController(
			UserManager<ApplicationUser> userManager,
			UserRepository userRepository,
			IDbConnection dbConnection,
			ProductRepository repository,
			Mapper mapper,
			ILogger<ProductController> logger)
			: base(userManager, userRepository, mapper)
		{
			_dbConnection = dbConnection;
			_repository = repository;
			_logger = logger;
		}

		[HttpGet("{id:guid}")]
		[ProducesResponseType(Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)]
		public async Task<ActionResult<ProductModel>> Get(Guid id, CancellationToken cancellationToken)
		{
			return await Find(() => _repository.FindByIdAsync(id, cancellationToken));
		}

		[HttpGet]
		[ProducesResponseType(Status200OK)]
		public async Task<ActionResult<List<ProductModel>>> GetAll(CancellationToken cancellationToken)
		{
			var accounts = await _repository.GetAllAsync(cancellationToken);
			var models = accounts.Select(account => MapToModel(account)).ToList();
			return Ok(models);
		}

		[HttpPost]
		[ProducesResponseType(Status201Created)]
		public async Task<ActionResult<Guid>> Create([FromBody, BindRequired] ProductCreationModel creationModel)
		{
			var user = await GetCurrentUser();
			if (user is null)
			{
				return Unauthorized();
			}

			var product = Mapper.Map<Product>(creationModel) with
			{
				OwnerId = user.Id,
				CreatedByUserId = user.Id,
				ModifiedByUserId = user.Id,
				NormalizedName = creationModel.Name!.ToUpperInvariant(),
			};

			var id = await _repository.AddAsync(product);
			return CreatedAtAction(nameof(Get), new { id }, id);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			_dbConnection.Dispose();
			_repository.Dispose();
			base.Dispose(disposing);
		}
	}
}
