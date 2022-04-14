// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using AutoMapper;

using Gnomeshade.Data;
using Gnomeshade.Data.Identity;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using NodaTime;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Authentication;

/// <summary>User authentication and information.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public sealed class AuthenticationController : ControllerBase
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly UserUnitOfWork _userUnitOfWork;
	private readonly Mapper _mapper;
	private readonly JwtOptions _jwtOptions;
	private readonly JwtSecurityTokenHandler _securityTokenHandler;
	private readonly ProblemDetailsFactory _problemDetailFactory;

	/// <summary>Initializes a new instance of the <see cref="AuthenticationController"/> class.</summary>
	/// <param name="userManager">Identity user persistence store.</param>
	/// <param name="userUnitOfWork">Application user persistence store.</param>
	/// <param name="mapper">Repository and API model mapper.</param>
	/// <param name="jwtOptions">Built-in authentication options.</param>
	/// <param name="securityTokenHandler">JWT token writer.</param>
	/// <param name="problemDetailsFactory"><see cref="ProblemDetailsFactory"/>.</param>
	public AuthenticationController(
		UserManager<ApplicationUser> userManager,
		UserUnitOfWork userUnitOfWork,
		Mapper mapper,
		IOptions<JwtOptions> jwtOptions,
		JwtSecurityTokenHandler securityTokenHandler,
		ProblemDetailsFactory problemDetailsFactory)
	{
		_userManager = userManager;
		_userUnitOfWork = userUnitOfWork;
		_mapper = mapper;
		_jwtOptions = jwtOptions.Value;
		_securityTokenHandler = securityTokenHandler;
		_problemDetailFactory = problemDetailsFactory;
	}

	/// <summary>Authenticates a user using the specified login information.</summary>
	/// <param name="login">Information for logging in.</param>
	/// <returns>Information about the authentication section.</returns>
	[AllowAnonymous]
	[HttpPost]
	[ProducesResponseType(typeof(LoginResponse), Status200OK)]
	[ProducesResponseType(Status401Unauthorized)]
	public async Task<ActionResult<LoginResponse>> Login([FromBody] Login login)
	{
		var user = await _userManager.FindByNameAsync(login.Username);
		if (user is null || !await _userManager.CheckPasswordAsync(user, login.Password))
		{
			return Unauthorized();
		}

		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.Id),
			new(ClaimTypes.Name, user.UserName),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
		};

		var roles = await _userManager.GetRolesAsync(user);
		var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));
		claims.AddRange(roleClaims);

		var authSigningKey = _jwtOptions.SecurityKey;
		var token = new JwtSecurityToken(
			_jwtOptions.ValidIssuer,
			_jwtOptions.ValidAudience,
			claims,
			DateTime.Now,
			DateTime.Now.AddHours(3),
			new(authSigningKey, SecurityAlgorithms.HmacSha256));

		return Ok(new LoginResponse(_securityTokenHandler.WriteToken(token), Instant.FromDateTimeUtc(token.ValidTo)));
	}

	/// <summary>Registers a new user.</summary>
	/// <param name="registration">The information about the user to register.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[AllowAnonymous]
	[HttpPost]
	[ProducesResponseType(Status204NoContent)]
	public async Task<ActionResult> Register([FromBody] RegistrationModel registration)
	{
		var user = _mapper.Map<ApplicationUser>(registration);
		user.SecurityStamp = Guid.NewGuid().ToString();

		var creationResult = await _userManager.CreateAsync(user, registration.Password);
		if (!creationResult.Succeeded)
		{
			var problemDetails = creationResult.GetProblemDetails(_problemDetailFactory, HttpContext);
			return BadRequest(problemDetails);
		}

		var identityUser = await _userManager.FindByNameAsync(registration.Username);

		try
		{
			await _userUnitOfWork.CreateUserAsync(identityUser);
		}
		catch (Exception)
		{
			await _userManager.DeleteAsync(identityUser);
			throw;
		}

		return NoContent();
	}

	/// <summary>Registers or authenticates a user using and OIDC provider.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[HttpPost]
	public async Task<ActionResult> SocialRegister()
	{
		var providerKeyClaim = User.FindAll(ClaimTypes.NameIdentifier).Single();
		var existingUser =
			await _userManager.FindByLoginAsync(providerKeyClaim.OriginalIssuer, providerKeyClaim.Value);
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

	/// <summary>Logs out the currently signed in user.</summary>
	/// <returns><see cref="SignOutResult"/>.</returns>
	[HttpPost]
	public SignOutResult Logout() => SignOut();
}
