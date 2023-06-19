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

/// <summary>Page for handling user login with 2FA recovery code.</summary>
public sealed class LoginWithRecoveryCode : PageModel
{
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly ILogger<LoginWithRecoveryCode> _logger;

	/// <summary>Initializes a new instance of the <see cref="LoginWithRecoveryCode"/> class.</summary>
	/// <param name="signInManager">Application user sign in manager.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	public LoginWithRecoveryCode(SignInManager<ApplicationUser> signInManager, ILogger<LoginWithRecoveryCode> logger)
	{
		_signInManager = signInManager;
		_logger = logger;
	}

	/// <summary>Gets or sets data needed to login with 2FA.</summary>
	[BindProperty]
	public InputModel Input { get; set; } = null!;

	/// <summary>Gets the return url parameter.</summary>
	public string? ReturnUrl { get; private set; }

	/// <summary>Handles requests made by the user.</summary>
	/// <param name="returnUrl">The URL to which to return to after successful login.</param>
	public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
	{
		// Ensure the user has gone through the username & password screen first
		var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
		if (user is null)
		{
			return RedirectToPage("./Login", new { returnUrl });
		}

		ReturnUrl = returnUrl;

		return Page();
	}

	/// <summary>Handles login attempt by the user.</summary>
	/// <param name="returnUrl">The URL to which to return to after successful login.</param>
	public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
		if (user is null)
		{
			throw new InvalidOperationException("Unable to load two-factor authentication user.");
		}

		var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

		var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

		if (result.Succeeded)
		{
			_logger.UserLoggedInRecoveryCode(user.Id);
			return LocalRedirect(returnUrl ?? Url.Content("~/"));
		}

		if (result.IsLockedOut)
		{
			_logger.UserLockedOut(user.Id);
			return RedirectToPage("./Lockout");
		}

		_logger.InvalidRecoveryCode(user.Id);
		ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
		return Page();
	}

	/// <summary>Data needed to recover an account with 2FA.</summary>
	[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
	public sealed class InputModel
	{
		/// <summary>Gets or sets the 2FA recovery code.</summary>
		[BindProperty]
		[Required]
		[DataType(DataType.Text)]
		[Display(Name = "Recovery Code")]
		public string RecoveryCode { get; set; } = null!;
	}
}
