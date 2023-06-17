// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account.Manage;

public sealed class Disable2Fa : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ILogger<Disable2Fa> _logger;

	public Disable2Fa(UserManager<ApplicationUser> userManager, ILogger<Disable2Fa> logger)
	{
		_userManager = userManager;
		_logger = logger;
	}

	[TempData]
	public string? StatusMessage { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		if (!await _userManager.GetTwoFactorEnabledAsync(user))
		{
			throw new InvalidOperationException($"Cannot disable 2FA for user as it's not currently enabled.");
		}

		return Page();
	}

	public async Task<IActionResult> OnPostAsync()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		var disable2FaResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
		if (!disable2FaResult.Succeeded)
		{
			throw new InvalidOperationException($"Unexpected error occurred disabling 2FA.");
		}

		_logger.LogInformation("User with ID '{UserId}' has disabled 2fa", _userManager.GetUserId(User));
		StatusMessage = "2fa has been disabled. You can reenable 2fa when you setup an authenticator app";
		return RedirectToPage("./TwoFactorAuthentication");
	}
}
