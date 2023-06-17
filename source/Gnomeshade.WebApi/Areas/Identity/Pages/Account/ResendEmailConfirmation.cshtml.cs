// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class ResendEmailConfirmation : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IEmailSender _emailSender;

	public ResendEmailConfirmation(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
	{
		_userManager = userManager;
		_emailSender = emailSender;
	}

	[BindProperty]
	public InputModel Input { get; set; } = null!;

	public void OnGet()
	{
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
			ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
			return Page();
		}

		var userId = await _userManager.GetUserIdAsync(user);
		var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
		code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
		var callbackUrl = Url.Page(
				"/Account/ConfirmEmail",
				null,
				new { userId, code },
				Request.Scheme) ??
			throw new InvalidOperationException("Failed to generate callback url");

		await _emailSender.SendEmailAsync(
			Input.Email,
			"Confirm your email",
			$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

		ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
		return Page();
	}

	public sealed class InputModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; init; } = null!;
	}
}
