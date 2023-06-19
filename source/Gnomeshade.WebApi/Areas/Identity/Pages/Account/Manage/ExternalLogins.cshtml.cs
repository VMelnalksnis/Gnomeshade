// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;
using Gnomeshade.WebApi.Configuration;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account.Manage;

public sealed class ExternalLogins : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly IUserStore<ApplicationUser> _userStore;

	public ExternalLogins(
		UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signInManager,
		IUserStore<ApplicationUser> userStore)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_userStore = userStore;
	}

	public IList<UserLoginInfo>? CurrentLogins { get; set; }

	public IList<AuthenticationScheme>? OtherLogins { get; set; }

	public bool ShowRemoveButton { get; set; }

	[TempData]
	public string? StatusMessage { get; set; }

	public async Task<IActionResult> OnGetAsync()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		CurrentLogins = await _userManager.GetLoginsAsync(user);
		OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
			.Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
			.ToList();

		string? passwordHash = null;
		if (_userStore is IUserPasswordStore<ApplicationUser> userPasswordStore)
		{
			passwordHash = await userPasswordStore.GetPasswordHashAsync(user, HttpContext.RequestAborted);
		}

		ShowRemoveButton = passwordHash is not null || CurrentLogins.Count > 1;
		return Page();
	}

	public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
	{
		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		var result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
		if (!result.Succeeded)
		{
			StatusMessage = "The external login was not removed.";
			return RedirectToPage();
		}

		await _signInManager.RefreshSignInAsync(user);
		StatusMessage = "The external login was removed.";
		return RedirectToPage();
	}

	public async Task<IActionResult> OnPostLinkLoginAsync(string provider)
	{
		// Clear the existing external cookie to ensure a clean login process
		await HttpContext.SignOutAsync(Schemes.External);

		// Request a redirect to the external login provider to link a login for the current user
		var redirectUrl = Url.Page("./ExternalLogins", "LinkLoginCallback");
		var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
		return new ChallengeResult(provider, properties);
	}

	public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		var userId = await _userManager.GetUserIdAsync(user);
		var info = await _signInManager.GetExternalLoginInfoAsync(userId);
		if (info is null)
		{
			throw new InvalidOperationException($"Unexpected error occurred loading external login info.");
		}

		var result = await _userManager.AddLoginAsync(user, info);
		if (!result.Succeeded)
		{
			StatusMessage =
				"The external login was not added. External logins can only be associated with one account.";
			return RedirectToPage();
		}

		// Clear the existing external cookie to ensure a clean login process
		await HttpContext.SignOutAsync(Schemes.External);

		StatusMessage = "The external login was added.";
		return RedirectToPage();
	}
}
