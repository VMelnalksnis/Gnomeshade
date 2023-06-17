// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account;

/// <summary>Page for handling user login.</summary>
public sealed class Login : PageModel
{
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly ILogger<Login> _logger;

	/// <summary>Initializes a new instance of the <see cref="Login"/> class.</summary>
	/// <param name="signInManager">Application user sign in manager.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	public Login(SignInManager<ApplicationUser> signInManager, ILogger<Login> logger)
	{
		_signInManager = signInManager;
		_logger = logger;
	}

	/// <summary>Gets or sets the data needed to login.</summary>
	[BindProperty]
	public InputModel Input { get; set; } = null!;

	/// <summary>Gets a collection of external authentication schemes.</summary>
	/// <seealso cref="ExternalLogin"/>
	public IList<AuthenticationScheme>? ExternalLogins { get; private set; }

	/// <summary>Gets the return url parameter.</summary>
	public string? ReturnUrl { get; private set; }

	/// <summary>Gets or sets the error message.</summary>
	[TempData]
	public string? ErrorMessage { get; set; }

	/// <summary>Handles requests made by the user.</summary>
	/// <param name="returnUrl">The URL to which to return to after successful login.</param>
	public async Task OnGetAsync(string? returnUrl = null)
	{
		if (!string.IsNullOrEmpty(ErrorMessage))
		{
			ModelState.AddModelError(string.Empty, ErrorMessage);
		}

		returnUrl ??= Url.Content("~/");

		// Clear the existing external cookie to ensure a clean login process
		await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

		ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

		ReturnUrl = returnUrl;
	}

	/// <summary>Handles login attempt by the user.</summary>
	/// <param name="returnUrl">The URL to which to return to after successful login.</param>
	public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
	{
		returnUrl ??= Url.Content("~/");

		ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

		if (!ModelState.IsValid)
		{
			return Page();
		}

		// This doesn't count login failures towards account lockout
		// To enable password failures to trigger account lockout, set lockoutOnFailure: true
		var result = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, Input.RememberMe, false);
		LogMessages.PasswordSignIn(_logger, result);
		if (result.Succeeded)
		{
			LogMessages.UserLoggedIn(_logger);
			return LocalRedirect(returnUrl);
		}

		if (result.RequiresTwoFactor)
		{
			return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
		}

		if (result.IsLockedOut)
		{
			LogMessages.UserLockedOut(_logger);
			return RedirectToPage("./Lockout");
		}

		ModelState.AddModelError(string.Empty, "Invalid login attempt.");
		return Page();
	}

	/// <summary>Data needed to log in a user.</summary>
	[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
	public sealed class InputModel
	{
		/// <summary>Gets or sets the username of the user.</summary>
		[Required]
		public string Username { get; set; } = null!;

		/// <summary>Gets or sets the password of the user.</summary>
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; } = null!;

		/// <summary>Gets or sets a value indicating whether to persist this login.</summary>
		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; }
	}
}
