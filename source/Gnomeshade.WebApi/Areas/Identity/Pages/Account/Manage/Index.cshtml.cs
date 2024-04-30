// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account.Manage;

public sealed class Index : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;

	public Index(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
	{
		_userManager = userManager;
		_signInManager = signInManager;
	}

	public string? Username { get; set; }

	[TempData]
	public string? StatusMessage { get; set; }

	[BindProperty]
	public InputModel Input { get; set; } = null!;

	public async Task<IActionResult> OnGetAsync()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		await LoadAsync(user);
		return Page();
	}

	public async Task<IActionResult> OnPostAsync()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		if (!ModelState.IsValid)
		{
			await LoadAsync(user);
			return Page();
		}

		await _signInManager.RefreshSignInAsync(user);
		StatusMessage = "Your profile has been updated";
		return RedirectToPage();
	}

	private async Task LoadAsync(ApplicationUser user)
	{
		var userName = await _userManager.GetUserNameAsync(user);

		Username = userName;

		Input = new();
	}

	public sealed class InputModel;
}
