// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Text;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account;

public sealed class ConfirmEmailChange : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;

	public ConfirmEmailChange(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
	{
		_userManager = userManager;
		_signInManager = signInManager;
	}

	[TempData]
	public string? StatusMessage { get; set; }

	public async Task<IActionResult> OnGetAsync(string? userId, string? email, string? code)
	{
		if (userId is null || email is null || code is null)
		{
			return RedirectToPage("/Index");
		}

		var user = await _userManager.FindByIdAsync(userId);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{userId}'.");
		}

		code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
		var result = await _userManager.ChangeEmailAsync(user, email, code);
		if (!result.Succeeded)
		{
			StatusMessage = "Error changing email.";
			return Page();
		}

		// In our UI email and user name are one and the same, so when we update the email
		// we need to update the user name.
		var setUserNameResult = await _userManager.SetUserNameAsync(user, email);
		if (!setUserNameResult.Succeeded)
		{
			StatusMessage = "Error changing user name.";
			return Page();
		}

		await _signInManager.RefreshSignInAsync(user);
		StatusMessage = "Thank you for confirming your email change.";
		return Page();
	}
}
