// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account.Manage;

public sealed class PersonalData : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;

	public PersonalData(UserManager<ApplicationUser> userManager)
	{
		_userManager = userManager;
	}

	public async Task<IActionResult> OnGet()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		return Page();
	}
}
