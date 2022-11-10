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
using Gnomeshade.WebApi.Configuration.Options;
using Gnomeshade.WebApi.Models.Authentication;
using Gnomeshade.WebApi.V1.Authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using NodaTime;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Gnomeshade.WebApi.V1.Controllers;

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
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
		};

		if (!string.IsNullOrWhiteSpace(user.UserName))
		{
			claims.Add(new(ClaimTypes.Name, user.UserName));
		}

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

		var identityUser = await _userManager.FindByNameAsync(registration.Username)
			?? throw new NullReferenceException();

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

	/// <summary>Logs out the currently signed in user.</summary>
	/// <returns><see cref="SignOutResult"/>.</returns>
	[HttpPost]
	public SignOutResult Logout() => SignOut();
}
