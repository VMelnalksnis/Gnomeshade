// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account.Manage;

public sealed class GenerateRecoveryCodes : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ILogger<GenerateRecoveryCodes> _logger;

	public GenerateRecoveryCodes(UserManager<ApplicationUser> userManager, ILogger<GenerateRecoveryCodes> logger)
	{
		_userManager = userManager;
		_logger = logger;
	}

	[TempData]
	public string[]? RecoveryCodes { get; set; }

	[TempData]
	public string? StatusMessage { get; set; }

	public async Task<IActionResult> OnGetAsync()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		if (!await _userManager.GetTwoFactorEnabledAsync(user))
		{
			return RedirectToPage("TwoFactorAuthentication");
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

		if (!await _userManager.GetTwoFactorEnabledAsync(user))
		{
			return RedirectToPage("TwoFactorAuthentication");
		}

		var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10) ??
				throw new InvalidOperationException("Failed to generate recovery codes");

		RecoveryCodes = recoveryCodes.ToArray();

		_logger.UserGenerated2FaCodes(user.Id);
		StatusMessage = "You have generated new recovery codes.";
		return RedirectToPage("./ShowRecoveryCodes");
	}
}
