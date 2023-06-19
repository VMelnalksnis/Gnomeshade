// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account;

/// <summary>Page for handling user login with 2FA.</summary>
public sealed class LoginWith2Fa : PageModel
{
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly ILogger<LoginWith2Fa> _logger;

	/// <summary>Initializes a new instance of the <see cref="LoginWith2Fa"/> class.</summary>
	/// <param name="signInManager">Application user sign in manager.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	public LoginWith2Fa(SignInManager<ApplicationUser> signInManager, ILogger<LoginWith2Fa> logger)
	{
		_signInManager = signInManager;
		_logger = logger;
	}

	/// <summary>Gets or sets the data needed to login.</summary>
	[BindProperty]
	public InputModel Input { get; set; } = null!;

	/// <inheritdoc cref="Login.InputModel.RememberMe"/>
	public bool RememberMe { get; private set; }

	/// <summary>Gets the return url parameter.</summary>
	public string? ReturnUrl { get; private set; }

	/// <summary>Handles requests made by the user.</summary>
	/// <param name="rememberMe">Whether to persist this login.</param>
	/// <param name="returnUrl">The URL to which to return to after successful login.</param>
	public async Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = null)
	{
		// Ensure the user has gone through the username & password screen first
		var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
		if (user is null)
		{
			return RedirectToPage("./Login", new { returnUrl });
		}

		ReturnUrl = returnUrl;
		RememberMe = rememberMe;

		return Page();
	}

	/// <summary>Handles login attempt by the user.</summary>
	/// <param name="rememberMe">Whether to persist this login.</param>
	/// <param name="returnUrl">The URL to which to return to after successful login.</param>
	public async Task<IActionResult> OnPostAsync(bool rememberMe, string? returnUrl = null)
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		returnUrl ??= Url.Content("~/");

		var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
		if (user is null)
		{
			throw new InvalidOperationException("Unable to load two-factor authentication user.");
		}

		var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

		var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);

		if (result.Succeeded)
		{
			_logger.UserLoggedIn2Fa(user.Id);
			return LocalRedirect(returnUrl);
		}

		if (result.IsLockedOut)
		{
			_logger.UserLockedOut(user.Id);
			return RedirectToPage("./Lockout");
		}

		_logger.InvalidAuthenticatorCode(user.Id);
		ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
		return Page();
	}

	/// <summary>Data needed to login with 2FA.</summary>
	[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
	public sealed class InputModel
	{
		/// <summary>Gets or sets the 2FA code.</summary>
		[Required]
		[StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
		[DataType(DataType.Text)]
		[Display(Name = "Authenticator code")]
		public string TwoFactorCode { get; set; } = null!;

		/// <summary>Gets or sets a value indicating whether to persist this login.</summary>
		[Display(Name = "Remember this machine")]
		public bool RememberMachine { get; set; }
	}
}
