// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account.Manage;

public sealed class DeletePersonalData : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly ILogger<DeletePersonalData> _logger;

	public DeletePersonalData(
		UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signInManager,
		ILogger<DeletePersonalData> logger)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_logger = logger;
	}

	[BindProperty]
	public InputModel Input { get; set; } = null!;

	public bool RequirePassword { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		RequirePassword = await _userManager.HasPasswordAsync(user);
		return Page();
	}

	public async Task<IActionResult> OnPostAsync()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		RequirePassword = await _userManager.HasPasswordAsync(user);
		if (RequirePassword)
		{
			if (!await _userManager.CheckPasswordAsync(user, Input.Password))
			{
				ModelState.AddModelError(string.Empty, "Incorrect password.");
				return Page();
			}
		}

		var result = await _userManager.DeleteAsync(user);
		var userId = await _userManager.GetUserIdAsync(user);
		if (!result.Succeeded)
		{
			throw new InvalidOperationException($"Unexpected error occurred deleting user.");
		}

		await _signInManager.SignOutAsync();

		_logger.LogInformation("User with ID '{UserId}' deleted themselves", userId);

		return Redirect("~/");
	}

	public sealed class InputModel
	{
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; init; } = null!;
	}
}
