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

public sealed class ConfirmEmail : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;

	public ConfirmEmail(UserManager<ApplicationUser> userManager)
	{
		_userManager = userManager;
	}

	[TempData]
	public string? StatusMessage { get; set; }

	public async Task<IActionResult> OnGetAsync(string? userId, string? code)
	{
		if (userId is null || code is null)
		{
			return RedirectToPage("/Index");
		}

		var user = await _userManager.FindByIdAsync(userId);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{userId}'.");
		}

		code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
		var result = await _userManager.ConfirmEmailAsync(user, code);
		StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
		return Page();
	}
}
