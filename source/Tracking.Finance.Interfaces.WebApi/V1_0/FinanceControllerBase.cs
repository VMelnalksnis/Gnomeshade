// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Tracking.Finance.Data.Identity;
using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Models.Abstractions;
using Tracking.Finance.Data.Repositories;

namespace Tracking.Finance.Interfaces.WebApi.V1_0
{
	[ApiController]
	[ApiVersion("1.0")]
	[Authorize]
	[Route("api/v{version:apiVersion}/[controller]")]
	[SuppressMessage(
		"ReSharper",
		"AsyncConverter.ConfigureAwaitHighlighting",
		Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
	public abstract class FinanceControllerBase<TEntity, TModel> : ControllerBase, IDisposable
		where TEntity : class, IEntity
		where TModel : class
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly UserRepository _userRepository;

		protected FinanceControllerBase(
			UserManager<ApplicationUser> userManager,
			UserRepository userRepository,
			Mapper mapper)
		{
			_userManager = userManager;
			_userRepository = userRepository;
			Mapper = mapper;
		}

		protected Mapper Mapper { get; }

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			_userManager.Dispose();
			_userRepository.Dispose();
		}

		protected async Task<User?> GetCurrentUser()
		{
			var identityUser = await _userManager.GetUserAsync(User);
			if (identityUser is null)
			{
				return null;
			}

			return await _userRepository.FindByIdAsync(new(identityUser.Id));
		}

		/// <summary>
		/// Finds a <typeparamref name="TModel"/> by the specified <paramref name="selector"/>.
		/// </summary>
		/// <param name="selector">Asynchronous function for finding an instance of <typeparamref name="TEntity"/>.</param>
		/// <returns><see cref="OkObjectResult"/> if an instance of <typeparamref name="TModel"/> was found, otherwise <see cref="NotFoundResult"/>.</returns>
		protected async Task<ActionResult<TModel>> Find(Func<Task<TEntity?>> selector)
		{
			var entity = await selector();
			if (entity is null)
			{
				return NotFound();
			}

			var model = MapToModel(entity);
			return Ok(model);
		}

		protected TModel MapToModel(TEntity entity) => Mapper.Map<TModel>(entity);
	}
}
