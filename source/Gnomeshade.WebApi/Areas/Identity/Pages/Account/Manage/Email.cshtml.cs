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

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account.Manage;

public sealed class Email : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IEmailSender _emailSender;

	public Email(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
	{
		_userManager = userManager;
		_emailSender = emailSender;
	}

	public string? CurrentEmail { get; set; }

	public bool IsEmailConfirmed { get; set; }

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

	public async Task<IActionResult> OnPostChangeEmailAsync()
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

		var email = await _userManager.GetEmailAsync(user);
		if (Input.NewEmail != email)
		{
			var userId = await _userManager.GetUserIdAsync(user);
			var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
			var callbackUrl = Url.Page(
					"/Account/ConfirmEmailChange",
					null,
					new { area = "Identity", userId, email = Input.NewEmail, code },
					Request.Scheme) ??
				throw new InvalidOperationException("Failed to generate callback url");

			await _emailSender.SendEmailAsync(
				Input.NewEmail,
				"Confirm your email",
				$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

			StatusMessage = "Confirmation link to change email sent. Please check your email.";
			return RedirectToPage();
		}

		StatusMessage = "Your email is unchanged.";
		return RedirectToPage();
	}

	public async Task<IActionResult> OnPostSendVerificationEmailAsync()
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

		var userId = await _userManager.GetUserIdAsync(user);
		var email = await _userManager.GetEmailAsync(user) ??
			throw new InvalidOperationException("User does not have an email");

		var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
		code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
		var callbackUrl = Url.Page(
				"/Account/ConfirmEmail",
				null,
				new { area = "Identity", userId, code },
				Request.Scheme) ??
			throw new InvalidOperationException("Failed to generate callback url");

		await _emailSender.SendEmailAsync(
			email,
			"Confirm your email",
			$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

		StatusMessage = "Verification email sent. Please check your email.";
		return RedirectToPage();
	}

	private async Task LoadAsync(ApplicationUser user)
	{
		var email = await _userManager.GetEmailAsync(user);
		CurrentEmail = email;

		Input = new() { NewEmail = email ?? string.Empty };

		IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
	}

	public sealed class InputModel
	{
		[Required]
		[EmailAddress]
		[Display(Name = "New email")]
		public string NewEmail { get; init; } = null!;
	}
}
