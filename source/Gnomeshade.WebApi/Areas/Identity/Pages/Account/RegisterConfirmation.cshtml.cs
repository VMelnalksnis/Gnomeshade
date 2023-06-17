// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Text;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class RegisterConfirmation : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;

	public RegisterConfirmation(UserManager<ApplicationUser> userManager)
	{
		_userManager = userManager;
	}

	public string? Email { get; set; }

	public bool DisplayConfirmAccountLink { get; set; }

	public string? EmailConfirmationUrl { get; set; }

	public async Task<IActionResult> OnGetAsync(string? email, string? returnUrl = null)
	{
		if (email is null)
		{
			return RedirectToPage("/Index");
		}

		returnUrl ??= Url.Content("~/");

		var user = await _userManager.FindByEmailAsync(email);
		if (user is null)
		{
			return NotFound($"Unable to load user with email '{email}'.");
		}

		Email = email;

		// Once you add a real email sender, you should remove this code that lets you confirm the account
		DisplayConfirmAccountLink = true;
		if (!DisplayConfirmAccountLink)
		{
			return Page();
		}

		var userId = await _userManager.GetUserIdAsync(user);
		var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
		code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
		EmailConfirmationUrl = Url.Page(
			"/Account/ConfirmEmail",
			null,
			new
			{
				area = "Identity",
				userId,
				code,
				returnUrl,
			},
			Request.Scheme);

		return Page();
	}
}
