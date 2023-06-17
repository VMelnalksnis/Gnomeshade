// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account;

public sealed class ForgotPassword : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IEmailSender _emailSender;

	public ForgotPassword(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
	{
		_userManager = userManager;
		_emailSender = emailSender;
	}

	[BindProperty]
	public InputModel Input { get; set; } = null!;

	public async Task<IActionResult> OnPostAsync()
	{
		if (ModelState.IsValid)
		{
			var user = await _userManager.FindByEmailAsync(Input.Email);
			if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
			{
				// Don't reveal that the user does not exist or is not confirmed
				return RedirectToPage("./ForgotPasswordConfirmation");
			}

			// For more information on how to enable account confirmation and password reset please
			// visit https://go.microsoft.com/fwlink/?LinkID=532713
			var code = await _userManager.GeneratePasswordResetTokenAsync(user);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
			var callbackUrl = Url.Page(
					"/Account/ResetPassword",
					null,
					new { area = "Identity", code },
					Request.Scheme) ??
				throw new InvalidOperationException("Failed to generate callback url");

			await _emailSender.SendEmailAsync(
				Input.Email,
				"Reset Password",
				$"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

			return RedirectToPage("./ForgotPasswordConfirmation");
		}

		return Page();
	}

	public sealed class InputModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; init; } = null!;
	}
}
