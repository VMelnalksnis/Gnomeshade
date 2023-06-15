// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Security.Claims;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;
using Gnomeshade.WebApi.V1.Authentication;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Gnomeshade.WebApi.V1.Controllers;

/// <summary>External authentication endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public sealed class ExternalAuthenticationController : ControllerBase
{
	private readonly UserManager<ApplicationUser> _userManager;

	/// <summary>Initializes a new instance of the <see cref="ExternalAuthenticationController"/> class.</summary>
	/// <param name="userManager">Identity user persistence store.</param>
	public ExternalAuthenticationController(UserManager<ApplicationUser> userManager)
	{
		_userManager = userManager;
	}

	/// <summary>Registers or authenticates a user using and OIDC provider.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[HttpPost]
	public async Task<ActionResult> SocialRegister()
	{
		var loginProvider = User.GetLoginProvider();
		var providerKeyClaim = User.GetSingleOrDefaultClaim(ClaimTypes.NameIdentifier);

		if (loginProvider is null || providerKeyClaim is null)
		{
			return StatusCode(StatusCodes.Status401Unauthorized);
		}

		var applicationUser = await _userManager.FindByLoginAsync(loginProvider, providerKeyClaim.Value);

		return applicationUser is not null
			? NoContent()
			: LocalRedirect("/Identity/Account/Register");
	}
}
