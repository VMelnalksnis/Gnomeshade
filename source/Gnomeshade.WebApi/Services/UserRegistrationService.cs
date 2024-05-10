// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Data;
using Gnomeshade.Data.Identity;
using Gnomeshade.WebApi.Areas.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.Services;

/// <summary>Service for register new users.</summary>
public sealed class UserRegistrationService
{
	private readonly ILogger<UserRegistrationService> _logger;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly UserUnitOfWork _userUnitOfWork;

	/// <summary>Initializes a new instance of the <see cref="UserRegistrationService"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="userManager">Application user manager.</param>
	/// <param name="userUnitOfWork">Application user persistence store.</param>
	public UserRegistrationService(
		ILogger<UserRegistrationService> logger,
		UserManager<ApplicationUser> userManager,
		UserUnitOfWork userUnitOfWork)
	{
		_logger = logger;
		_userManager = userManager;
		_userUnitOfWork = userUnitOfWork;
	}

	/// <summary>Register a new user.</summary>
	/// <param name="username">The unique username of the user.</param>
	/// <param name="fullName">The full name of the user for display.</param>
	/// <param name="password">The password of the user.</param>
	/// <returns>An <see cref="IdentityResult"/>.</returns>
	public async Task<IdentityResult> RegisterUser(string username, string fullName, string password)
	{
		var user = new ApplicationUser(username)
		{
			FullName = fullName,
		};

		var result = await _userManager.CreateAsync(user, password);
		if (!result.Succeeded)
		{
			return result;
		}

		_logger.UserCreated();

		var identityUser = await _userManager.FindByNameAsync(username);
		if (identityUser is null)
		{
			throw new InvalidOperationException("Could not find user by name after creating it");
		}

		try
		{
			await _userUnitOfWork.CreateUserAsync(identityUser);
		}
		catch (Exception)
		{
			await _userManager.DeleteAsync(identityUser);
			throw;
		}

		if (_userManager.Options.SignIn.RequireConfirmedAccount)
		{
			throw new NotSupportedException("Email confirmation is not supported");
		}

		return result;
	}
}
