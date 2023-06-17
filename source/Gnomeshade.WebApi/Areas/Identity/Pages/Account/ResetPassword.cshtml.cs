// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account;

public sealed class ResetPassword : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;

	public ResetPassword(UserManager<ApplicationUser> userManager)
	{
		_userManager = userManager;
	}

	[BindProperty]
	public InputModel Input { get; set; } = null!;

	public IActionResult OnGet(string? code = null)
	{
		if (code is null)
		{
			return BadRequest("A code must be supplied for password reset.");
		}

		Input = new()
		{
			Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)),
		};
		return Page();
	}

	public async Task<IActionResult> OnPostAsync()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var user = await _userManager.FindByEmailAsync(Input.Email);
		if (user is null)
		{
			// Don't reveal that the user does not exist
			return RedirectToPage("./ResetPasswordConfirmation");
		}

		var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
		if (result.Succeeded)
		{
			return RedirectToPage("./ResetPasswordConfirmation");
		}

		foreach (var error in result.Errors)
		{
			ModelState.AddModelError(string.Empty, error.Description);
		}

		return Page();
	}

	public sealed class InputModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; init; } = null!;

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		public string Password { get; init; } = null!;

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; init; } = null!;

		[Required]
		public string Code { get; init; } = null!;
	}
}
