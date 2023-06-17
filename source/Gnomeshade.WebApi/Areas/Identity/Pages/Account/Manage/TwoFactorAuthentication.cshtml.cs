// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account.Manage;

public sealed class TwoFactorAuthentication : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;

	public TwoFactorAuthentication(
		UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signInManager)
	{
		_userManager = userManager;
		_signInManager = signInManager;
	}

	public bool HasAuthenticator { get; set; }

	public int RecoveryCodesLeft { get; set; }

	[BindProperty]
	public bool Is2FaEnabled { get; set; }

	public bool IsMachineRemembered { get; set; }

	[TempData]
	public string? StatusMessage { get; set; }

	public async Task<IActionResult> OnGetAsync()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) is not null;
		Is2FaEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
		IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
		RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

		return Page();
	}

	public async Task<IActionResult> OnPostAsync()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		await _signInManager.ForgetTwoFactorClientAsync();
		StatusMessage =
			"The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";
		return RedirectToPage();
	}
}
