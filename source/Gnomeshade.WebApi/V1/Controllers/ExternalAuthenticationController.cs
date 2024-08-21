// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Security.Claims;
using System.Threading.Tasks;

using Asp.Versioning;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.OpenApi;
using Gnomeshade.WebApi.V1.Authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>External authentication endpoints.</summary>
[AllowAnonymous]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public sealed class ExternalAuthenticationController : ControllerBase
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly UserRepository _userRepository;

	/// <summary>Initializes a new instance of the <see cref="ExternalAuthenticationController"/> class.</summary>
	/// <param name="userManager">Identity user persistence store.</param>
	/// <param name="userRepository">The repository for performing CRUD operations on <see cref="UserEntity"/>.</param>
	public ExternalAuthenticationController(UserManager<ApplicationUser> userManager, UserRepository userRepository)
	{
		_userManager = userManager;
		_userRepository = userRepository;
	}

	/// <summary>Registers or authenticates a user using and OIDC provider.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[HttpPost]
	[ProducesResponseType(Status204NoContent)]
	[ProducesResponseType(Status302Found)]
	[ProducesStatus404NotFound]
	public async Task<ActionResult> SocialRegister()
	{
		var loginProvider = User.GetLoginProvider();
		var providerKeyClaim = User.GetSingleOrDefaultClaim(ClaimTypes.NameIdentifier);

		if (loginProvider is null || providerKeyClaim is null)
		{
			return StatusCode(Status401Unauthorized);
		}

		var applicationUser = await _userManager.FindByLoginAsync(loginProvider, providerKeyClaim.Value);
		if (applicationUser is null)
		{
			return Redirect("/Identity/Account/Register");
		}

		var user = await _userRepository.FindById(applicationUser.Id);
		return user is not null
			? NoContent()
			: Redirect("/Identity/Account/Register");
	}
}
