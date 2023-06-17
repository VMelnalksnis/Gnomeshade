// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account.Manage;

public sealed class ShowRecoveryCodes : PageModel
{
	[TempData]
	public string[]? RecoveryCodes { get; set; }

	[TempData]
	public string? StatusMessage { get; set; }

	public IActionResult OnGet()
	{
		if (RecoveryCodes is null || RecoveryCodes.Length == 0)
		{
			return RedirectToPage("./TwoFactorAuthentication");
		}

		return Page();
	}
}
