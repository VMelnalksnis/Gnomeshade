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

using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Models;
using Gnomeshade.Data.Repositories;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Products
{
	[SuppressMessage(
		"ReSharper",
		"AsyncConverter.ConfigureAwaitHighlighting",
		Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
	public sealed class UnitController : FinanceControllerBase<Unit, UnitModel>
	{
		private readonly IDbConnection _dbConnection;
		private readonly UnitRepository _repository;
		private readonly ILogger<UnitController> _logger;

		public UnitController(
			UserManager<ApplicationUser> userManager,
			UserRepository userRepository,
			IDbConnection dbConnection,
			UnitRepository repository,
			Mapper mapper,
			ILogger<UnitController> logger)
			: base(userManager, userRepository, mapper)
		{
			_dbConnection = dbConnection;
			_repository = repository;
			_logger = logger;
		}

		[HttpGet("{id:guid}")]
		[ProducesResponseType(Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), Status404NotFound)]
		public async Task<ActionResult<UnitModel>> Get(Guid id, CancellationToken cancellationToken)
		{
			return await Find(() => _repository.FindByIdAsync(id, cancellationToken));
		}

		[HttpGet]
		[ProducesResponseType(Status200OK)]
		public async Task<ActionResult<List<UnitModel>>> GetAll(CancellationToken cancellationToken)
		{
			var units = await _repository.GetAllAsync(cancellationToken);
			var models = units.Select(MapToModel).ToList();
			return Ok(models);
		}

		[HttpPost]
		[ProducesResponseType(Status201Created)]
		public async Task<ActionResult<Guid>> Create([FromBody, BindRequired] UnitCreationModel creationModel)
		{
			var user = await GetCurrentUser();
			if (user is null)
			{
				return Unauthorized();
			}

			var unit = Mapper.Map<Unit>(creationModel) with
			{
				OwnerId = user.Id,
				CreatedByUserId = user.Id,
				ModifiedByUserId = user.Id,
				NormalizedName = creationModel.Name!.ToUpperInvariant(),
			};

			var id = await _repository.AddAsync(unit);
			return CreatedAtAction(nameof(Get), new { id }, id);
		}
	}
}
