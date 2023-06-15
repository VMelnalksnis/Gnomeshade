// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Security.Claims;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Repositories;
using Gnomeshade.WebApi.V1.Authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Gnomeshade.WebApi.V1.Authorization;

/// <summary>
/// Authorization handler which handles the <see cref="ApplicationUserRequirement"/>.
/// </summary>
public sealed class ApplicationUserHandler : AuthorizationHandler<ApplicationUserRequirement>
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly UserRepository _userRepository;
	private readonly ApplicationUserContext _applicationUserContext;

	/// <summary>
	/// Initializes a new instance of the <see cref="ApplicationUserHandler"/> class.
	/// </summary>
	/// <param name="userManager">User manager for getting the identity user.</param>
	/// <param name="userRepository">User repository for getting the application user.</param>
	/// <param name="applicationUserContext">Context for providing information about the current user.</param>
	public ApplicationUserHandler(
		UserManager<ApplicationUser> userManager,
		UserRepository userRepository,
		ApplicationUserContext applicationUserContext)
	{
		_userManager = userManager;
		_userRepository = userRepository;
		_applicationUserContext = applicationUserContext;
	}

	/// <inheritdoc />
	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		ApplicationUserRequirement requirement)
	{
		var identityUser = await _userManager.GetUserAsync(context.User);
		var loginProvider = context.User.GetLoginProvider();
		var providerKeyClaim = context.User.GetSingleOrDefaultClaim(ClaimTypes.NameIdentifier);

		if (identityUser is null &&
			loginProvider is not null &&
			providerKeyClaim is not null)
		{
			identityUser = await _userManager.FindByLoginAsync(loginProvider, providerKeyClaim.Value);
		}

		if (identityUser is null)
		{
			context.Fail(new(this, "Failed to find identity user"));
			return;
		}

		var applicationUser = await _userRepository.FindByIdAsync(identityUser.Id);
		if (applicationUser is null)
		{
			context.Fail(new(this, "Failed to find application user"));
			return;
		}

		_applicationUserContext.User = applicationUser;
		context.Succeed(requirement);
	}
}
