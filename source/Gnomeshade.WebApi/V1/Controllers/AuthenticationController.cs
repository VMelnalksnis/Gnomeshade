// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.Configuration.Options;
using Gnomeshade.WebApi.Models.Authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
	private readonly UserRepository _userRepository;
	private readonly JwtOptions _jwtOptions;
	private readonly JwtSecurityTokenHandler _securityTokenHandler;

	/// <summary>Initializes a new instance of the <see cref="AuthenticationController"/> class.</summary>
	/// <param name="userManager">Identity user persistence store.</param>
	/// <param name="jwtOptions">Built-in authentication options.</param>
	/// <param name="securityTokenHandler">JWT token writer.</param>
	/// <param name="userRepository">The repository for performing CRUD operations on <see cref="UserEntity"/>.</param>
	public AuthenticationController(
		UserManager<ApplicationUser> userManager,
		IOptions<JwtOptions> jwtOptions,
		JwtSecurityTokenHandler securityTokenHandler,
		UserRepository userRepository)
	{
		_userManager = userManager;
		_jwtOptions = jwtOptions.Value;
		_securityTokenHandler = securityTokenHandler;
		_userRepository = userRepository;
	}

	/// <summary>Authenticates a user using the specified login information.</summary>
	/// <param name="login">Information for logging in.</param>
	/// <returns>Information about the authentication section.</returns>
	[AllowAnonymous]
	[HttpPost]
	[ProducesResponseType<LoginResponse>(Status200OK)]
	[ProducesResponseType(Status401Unauthorized)]
	public async Task<ActionResult<LoginResponse>> Login([FromBody] Login login)
	{
		var user = await _userManager.FindByNameAsync(login.Username);
		if (user is null ||
			!await _userManager.CheckPasswordAsync(user, login.Password) ||
			await _userRepository.FindById(user.Id) is null)
		{
			return Unauthorized();
		}

		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.Id.ConvertIdToString()),
			new(ClaimTypes.Name, user.FullName),
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

	/// <summary>Logs out the currently signed in user.</summary>
	/// <returns><see cref="SignOutResult"/>.</returns>
	[HttpPost]
	public SignOutResult Logout() => SignOut();
}
