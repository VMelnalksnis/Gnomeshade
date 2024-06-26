// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

using Gnomeshade.Data;
using Gnomeshade.Data.Identity;
using Gnomeshade.WebApi.V1.Authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account;

/// <summary>Page for handling authentication from an external provider.</summary>
[AllowAnonymous]
public sealed class ExternalLogin : PageModel
{
	private readonly ILogger<ExternalLogin> _logger;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly UserUnitOfWork _userUnitOfWork;

	/// <summary>Initializes a new instance of the <see cref="ExternalLogin"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="signInManager">Application user sign in manager.</param>
	/// <param name="userManager">Application user manager.</param>
	/// <param name="userUnitOfWork">Application user persistence store.</param>
	public ExternalLogin(
		ILogger<ExternalLogin> logger,
		SignInManager<ApplicationUser> signInManager,
		UserManager<ApplicationUser> userManager,
		UserUnitOfWork userUnitOfWork)
	{
		_logger = logger;
		_signInManager = signInManager;
		_userManager = userManager;
		_userUnitOfWork = userUnitOfWork;
	}

	/// <summary>Gets or sets the data needed to register a user from an external provider.</summary>
	[BindProperty]
	public InputModel Input { get; set; } = null!;

	/// <summary>Gets or sets the external authentication provider's name.</summary>
	public string ProviderDisplayName { get; set; } = null!;

	/// <summary>Gets or sets the return url parameter for external authentication.</summary>
	public string ReturnUrl { get; set; } = null!;

	/// <summary>Gets or sets the authentication error message.</summary>
	[TempData]
	public string? ErrorMessage { get; set; }

	/// <summary>Handles requests made by the user.</summary>
	public IActionResult OnGet() => RedirectToPage("./Login");

	/// <summary>Redirects to the external provider login page.</summary>
	/// <param name="provider">The name of the external provider.</param>
	/// <param name="returnUrl">The URL to which to return to after login.</param>
	public IActionResult OnPost(string provider, string? returnUrl = null)
	{
		// Request a redirect to the external login provider.
		var redirectUrl = Url.Page("./ExternalLogin", "Callback", new { returnUrl });
		var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
		return new ChallengeResult(provider, properties);
	}

	/// <summary>Handles registration form setup after being redirected from external provider.</summary>
	/// <param name="returnUrl">The URL to which to return to after registration.</param>
	/// <param name="remoteError">Error message from the external provider.</param>
	public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
	{
		returnUrl ??= Url.Content("~/");
		if (remoteError is not null)
		{
			ErrorMessage = $"Error from external provider: {remoteError}";
			return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
		}

		var info = await _signInManager.GetExternalLoginInfoAsync();
		if (info is null)
		{
			ErrorMessage = "Error loading external login information.";
			return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
		}

		_logger.RetrievedExternalUserInfo(info.LoginProvider, info.ProviderKey);

		// Sign in the user with this external login provider if the user already has a login.
		var result = await _signInManager.ExternalLoginSignInAsync(
			info.LoginProvider,
			info.ProviderKey,
			false,
			true);

		if (result.Succeeded)
		{
			_logger.UserLoggedInExternal(info.Principal.Identity?.Name, info.LoginProvider);
			return LocalRedirect(returnUrl);
		}

		if (result.IsLockedOut)
		{
			return RedirectToPage("./Lockout");
		}

		// If the user does not have an account, then ask the user to create an account.
		ReturnUrl = returnUrl;
		ProviderDisplayName = info.ProviderDisplayName ?? info.LoginProvider;
		Input = new();

		if (info.Principal.TryGetFirstClaimValue(ClaimTypes.Name, out var fullName))
		{
			Input.FullName = fullName;
		}
		else if (info.Principal.TryGetFirstClaimValue("name", out var alternativeFullName))
		{
			Input.FullName = alternativeFullName;
		}

		if (info.Principal.TryGetFirstClaimValue("preferred_username", out var username))
		{
			Input.UserName = username;
		}
		else if (info.Principal.TryGetFirstClaimValue(ClaimTypes.Email, out var email))
		{
			Input.UserName = email;
		}

		return Page();
	}

	/// <summary>Handles user registration after successfully returning from the external authentication provider.</summary>
	/// <param name="returnUrl">The URL to which to return to after registration.</param>
	public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
	{
		returnUrl ??= Url.Content("~/");

		// Get the information about the user from the external login provider
		var info = await _signInManager.GetExternalLoginInfoAsync();
		if (info is null)
		{
			ErrorMessage = "Error loading external login information during confirmation.";
			return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
		}

		ProviderDisplayName = info.ProviderDisplayName ?? info.LoginProvider;
		ReturnUrl = returnUrl;

		if (!ModelState.IsValid)
		{
			return Page();
		}

		if (!Guid.TryParseExact(info.ProviderKey, "D", out var id))
		{
			id = Guid.NewGuid();
		}

		var user = new ApplicationUser(Input.UserName)
		{
			Id = id,
			FullName = Input.FullName,
		};

		var result = await _userManager.CreateAsync(user);
		if (!result.Succeeded)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return Page();
		}

		result = await _userManager.AddLoginAsync(user, info);
		if (!result.Succeeded)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return Page();
		}

		_logger.UserCreatedExternal(info.LoginProvider);

		var identityUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
		if (identityUser is null)
		{
			throw new InvalidOperationException("Could not find user by login after creating and adding the login");
		}

		try
		{
			await _userUnitOfWork.CreateUserAsync(identityUser);
		}
		catch (Exception)
		{
			await _userManager.DeleteAsync(identityUser);
			throw;
		}

		// If account confirmation is required, we need to show the link if we don't have a real email sender
		if (_userManager.Options.SignIn.RequireConfirmedAccount)
		{
			throw new NotSupportedException("Email confirmation is not supported");
		}

		await _signInManager.SignInAsync(user, false, info.LoginProvider);
		return LocalRedirect(returnUrl);
	}

	/// <summary>Data needed to register a user from an external provider.</summary>
	public sealed class InputModel
	{
		/// <summary>Gets or sets the full name of the user.</summary>
		[Required]
		public string FullName { get; set; } = null!;

		/// <summary>Gets or sets the username of the user.</summary>
		[Required]
		public string UserName { get; set; } = null!;
	}
}
