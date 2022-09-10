// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Gnomeshade.Data;
using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Authentication;

/// <summary>External authentication endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public sealed class ExternalAuthenticationController : ControllerBase
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ProblemDetailsFactory _problemDetailFactory;
	private readonly UserUnitOfWork _userUnitOfWork;

	/// <summary>Initializes a new instance of the <see cref="ExternalAuthenticationController"/> class.</summary>
	/// <param name="userManager">Identity user persistence store.</param>
	/// <param name="problemDetailFactory"><see cref="ProblemDetailsFactory"/>.</param>
	/// <param name="userUnitOfWork">Application user persistence store.</param>
	public ExternalAuthenticationController(
		UserManager<ApplicationUser> userManager,
		ProblemDetailsFactory problemDetailFactory,
		UserUnitOfWork userUnitOfWork)
	{
		_userManager = userManager;
		_problemDetailFactory = problemDetailFactory;
		_userUnitOfWork = userUnitOfWork;
	}

	/// <summary>Registers or authenticates a user using and OIDC provider.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[HttpPost]
	public async Task<ActionResult> SocialRegister()
	{
		var providerKeyClaim = User.FindAll(ClaimTypes.NameIdentifier).Single();
		var existingUser = await _userManager.FindByLoginAsync(providerKeyClaim.OriginalIssuer, providerKeyClaim.Value);
		if (existingUser is not null)
		{
			return Ok();
		}

		var applicationUser = new ApplicationUser
		{
			Email = User.FindFirstValue(ClaimTypes.Email),
			FullName = User.FindFirstValue(ClaimTypes.Name),
			UserName = User.FindFirstValue("preferred_username"),
		};

		var userCreationResult = await _userManager.CreateAsync(applicationUser);
		if (!userCreationResult.Succeeded)
		{
			var problemDetails = userCreationResult.GetProblemDetails(_problemDetailFactory, HttpContext);
			return BadRequest(problemDetails);
		}

		var userLoginInfo = new UserLoginInfo(providerKeyClaim.OriginalIssuer, providerKeyClaim.Value, "Keycloak");
		var loginCreationResult = await _userManager.AddLoginAsync(applicationUser, userLoginInfo);
		if (!loginCreationResult.Succeeded)
		{
			var problemDetails = loginCreationResult.GetProblemDetails(_problemDetailFactory, HttpContext);
			return BadRequest(problemDetails);
		}

		var identityUser =
			await _userManager.FindByLoginAsync(providerKeyClaim.OriginalIssuer, providerKeyClaim.Value);

		try
		{
			await _userUnitOfWork.CreateUserAsync(identityUser);
		}
		catch (Exception)
		{
			await _userManager.DeleteAsync(identityUser);
			throw;
		}

		return StatusCode(Status201Created);
	}
}
