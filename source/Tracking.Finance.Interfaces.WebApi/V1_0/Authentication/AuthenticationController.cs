// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Tracking.Finance.Data.Identity;
using Tracking.Finance.Data.Models;
using Tracking.Finance.Data.Repositories;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Authentication
{
	[ApiController]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]/[action]")]
	[SuppressMessage(
		"ReSharper",
		"AsyncConverter.ConfigureAwaitHighlighting",
		Justification = "ASP.NET Core doesn't have a SynchronizationContext")]
	public sealed class AuthenticationController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IDbConnection _dbConnection;
		private readonly UserRepository _userRepository;
		private readonly OwnerRepository _ownerRepository;
		private readonly OwnershipRepository _ownershipRepository;
		private readonly Mapper _mapper;
		private readonly JwtOptions _jwtOptions;
		private readonly JwtSecurityTokenHandler _securityTokenHandler;
		private readonly ProblemDetailsFactory _problemDetailFactory;
		private readonly ILogger<AuthenticationController> _logger;

		public AuthenticationController(
			UserManager<ApplicationUser> userManager,
			IDbConnection dbConnection,
			UserRepository userRepository,
			OwnerRepository ownerRepository,
			OwnershipRepository ownershipRepository,
			Mapper mapper,
			IOptions<JwtOptions> jwtOptions,
			JwtSecurityTokenHandler securityTokenHandler,
			ProblemDetailsFactory problemDetailsFactory,
			ILogger<AuthenticationController> logger)
		{
			_userManager = userManager;
			_dbConnection = dbConnection;
			_userRepository = userRepository;
			_ownerRepository = ownerRepository;
			_ownershipRepository = ownershipRepository;
			_mapper = mapper;
			_jwtOptions = jwtOptions.Value;
			_securityTokenHandler = securityTokenHandler;
			_problemDetailFactory = problemDetailsFactory;
			_logger = logger;
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

			return Ok(new LoginResponse(_securityTokenHandler.WriteToken(token), token.ValidTo));
		}

		[HttpPost]
		[ProducesResponseType(Status200OK)]
		public async Task<ActionResult> Register([FromBody, BindRequired] RegistrationModel registration)
		{
			var user = _mapper.Map<ApplicationUser>(registration);
			user.SecurityStamp = Guid.NewGuid().ToString();

			// todo rollback if failed to create
			var creationResult = await _userManager.CreateAsync(user, registration.Password);
			if (!creationResult.Succeeded)
			{
				var problemDetails = creationResult.GetProblemDetails(_problemDetailFactory, HttpContext);
				return BadRequest(problemDetails);
			}

			var identityUser = await _userManager.FindByNameAsync(registration.Username);
			var identityUserId = new Guid(identityUser.Id);
			var applicationUser = new User { Id = identityUserId };
			_dbConnection.Open();
			using var dbTransaction = _dbConnection.BeginTransaction();

			try
			{
				_ = await _userRepository.AddWithIdAsync(applicationUser, dbTransaction);
				_ = await _ownerRepository.AddAsync(identityUserId, dbTransaction);
				_ = await _ownershipRepository.AddDefaultAsync(identityUserId, dbTransaction);
				dbTransaction.Commit();
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Failed creating user");
				dbTransaction.Rollback();
				throw;
			}

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
