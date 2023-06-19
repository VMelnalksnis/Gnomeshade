// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account.Manage;

public sealed class EnableAuthenticator : PageModel
{
	private const string _authenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ILogger<EnableAuthenticator> _logger;
	private readonly UrlEncoder _urlEncoder;

	public EnableAuthenticator(
		UserManager<ApplicationUser> userManager,
		ILogger<EnableAuthenticator> logger,
		UrlEncoder urlEncoder)
	{
		_userManager = userManager;
		_logger = logger;
		_urlEncoder = urlEncoder;
	}

	public string? SharedKey { get; set; }

	public string? AuthenticatorUri { get; set; }

	[TempData]
	public string[]? RecoveryCodes { get; set; }

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

		await LoadSharedKeyAndQrCodeUriAsync(user);

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
			await LoadSharedKeyAndQrCodeUriAsync(user);
			return Page();
		}

		// Strip spaces and hyphens
		var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

		var is2FaTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
			user,
			_userManager.Options.Tokens.AuthenticatorTokenProvider,
			verificationCode);

		if (!is2FaTokenValid)
		{
			ModelState.AddModelError("Input.Code", "Verification code is invalid.");
			await LoadSharedKeyAndQrCodeUriAsync(user);
			return Page();
		}

		await _userManager.SetTwoFactorEnabledAsync(user, true);
		_logger.UserEnabled2Fa(user.Id);

		StatusMessage = "Your authenticator app has been verified.";

		if (await _userManager.CountRecoveryCodesAsync(user) is not 0)
		{
			return RedirectToPage("./TwoFactorAuthentication");
		}

		var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
		RecoveryCodes = recoveryCodes?.ToArray() ??
			throw new InvalidOperationException("Failed to generate recovery codes");

		return RedirectToPage("./ShowRecoveryCodes");
	}

	private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user)
	{
		// Load the authenticator key & QR code URI to display on the form
		var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
		if (string.IsNullOrEmpty(unformattedKey))
		{
			await _userManager.ResetAuthenticatorKeyAsync(user);
			unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
		}

		if (string.IsNullOrEmpty(unformattedKey))
		{
			throw new InvalidOperationException("Failed to load authenticator key");
		}

		SharedKey = FormatKey(unformattedKey);

		var email = await _userManager.GetEmailAsync(user) ??
			throw new InvalidOperationException("User does not have an email");

		AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
	}

	private string FormatKey(string unformattedKey)
	{
		var result = new StringBuilder();
		var currentPosition = 0;
		while (currentPosition + 4 < unformattedKey.Length)
		{
			result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
			currentPosition += 4;
		}

		if (currentPosition < unformattedKey.Length)
		{
			result.Append(unformattedKey.AsSpan(currentPosition));
		}

		return result.ToString().ToLowerInvariant();
	}

	private string GenerateQrCodeUri(string email, string unformattedKey) => string.Format(
		CultureInfo.InvariantCulture,
		_authenticatorUriFormat,
		_urlEncoder.Encode("Microsoft.AspNetCore.Identity.UI"),
		_urlEncoder.Encode(email),
		unformattedKey);

	public sealed class InputModel
	{
		[Required]
		[StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
		[DataType(DataType.Text)]
		[Display(Name = "Verification Code")]
		public string Code { get; init; } = null!;
	}
}
