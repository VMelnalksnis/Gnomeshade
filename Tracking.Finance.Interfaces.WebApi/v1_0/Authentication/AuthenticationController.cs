// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Tracking.Finance.Data.Identity;
using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.Authentication
{
	[ApiController]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]/[action]")]
	public sealed class AuthenticationController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly UserRepository _userRepository;
		private readonly Mapper _mapper;
		private readonly JwtOptions _jwtOptions;
		private readonly JwtSecurityTokenHandler _securityTokenHandler;
		private readonly ProblemDetailsFactory _problemDetailFactory;

		public AuthenticationController(
			UserManager<ApplicationUser> userManager,
			UserRepository userRepository,
			Mapper mapper,
			IOptions<JwtOptions> jwtOptions,
			JwtSecurityTokenHandler securityTokenHandler,
			ProblemDetailsFactory problemDetailsFactory)
		{
			_userManager = userManager;
			_userRepository = userRepository;
			_mapper = mapper;
			_jwtOptions = jwtOptions.Value;
			_securityTokenHandler = securityTokenHandler;
			_problemDetailFactory = problemDetailsFactory;
		}

		[HttpPost]
		[ProducesResponseType(Status200OK)]
		[ProducesResponseType(Status401Unauthorized)]
		public async Task<ActionResult<LoginResponse>> Login([FromBody, BindRequired] LoginModel login)
		{
			var user = await _userManager.FindByNameAsync(login.Username);
			if (user is null || !await _userManager.CheckPasswordAsync(user, login.Password))
			{
				return Unauthorized();
			}

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
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
				new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

			return Ok(new LoginResponse(_securityTokenHandler.WriteToken(token), token.ValidTo));
		}

		[HttpPost]
		[ProducesResponseType(Status200OK)]
		public async Task<ActionResult> Register([FromBody, BindRequired] RegistrationModel registration)
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
			var applicationUser = new User { IdentityUserId = identityUser.Id };
			await _userRepository.AddAsync(applicationUser);

			return Ok();
		}

		[Authorize]
		[HttpGet]
		[ProducesResponseType(Status200OK)]
		public async Task<ActionResult<UserModel>> Info()
		{
			var identityUser = await _userManager.GetUserAsync(User);

			// var user = (await _userRepository.GetAllAsync()).Single(u => u.IdentityUserId == identityUser.Id);
			return Ok(_mapper.Map<UserModel>(identityUser));
		}
	}
}
