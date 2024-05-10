// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;
using Gnomeshade.WebApi.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account;

/// <summary>Page for handling new user registration.</summary>
[AllowAnonymous]
public sealed class Register : PageModel
{
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserRegistrationService _registrationService;

	/// <summary>Initializes a new instance of the <see cref="Register"/> class.</summary>
	/// <param name="signInManager">Application user sign in manager.</param>
	/// <param name="registrationService">Application user registration service.</param>
	public Register(SignInManager<ApplicationUser> signInManager, UserRegistrationService registrationService)
	{
		_signInManager = signInManager;
		_registrationService = registrationService;
	}

	/// <summary>Gets or sets the data needed to register a user.</summary>
	[BindProperty]
	public InputModel Input { get; set; } = null!;

	/// <summary>Gets the return url parameter.</summary>
	public string? ReturnUrl { get; private set; }

	/// <summary>Gets a collection of external authentication schemes.</summary>
	/// <seealso cref="ExternalLogin"/>
	public IList<AuthenticationScheme>? ExternalLogins { get; private set; }

	/// <summary>Handles requests made by the user.</summary>
	/// <param name="returnUrl">The URL to which to return to after registration.</param>
	public async Task OnGetAsync(string? returnUrl = null)
	{
		ReturnUrl = returnUrl;
		ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
	}

	/// <summary>Handles registration attempt by the user.</summary>
	/// <param name="returnUrl">The URL to which to return to after registration.</param>
	public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
	{
		returnUrl ??= Url.Content("~/");
		ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
		if (ModelState.IsValid)
		{
			var result = await _registrationService.RegisterUser(Input.UserName, Input.FullName, Input.Password);
			if (result.Succeeded)
			{
				await _signInManager.SignInAsync(new(Input.UserName), false);
				return LocalRedirect(returnUrl);
			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}

		// If we got this far, something failed, redisplay form
		return Page();
	}

	/// <summary>Data needed to register a user.</summary>
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
	public sealed class InputModel
	{
		/// <summary>Gets or sets the full name of the user.</summary>
		[Required]
		[Display(Name = "Full Name")]
		public string FullName { get; set; } = null!;

		/// <summary>Gets or sets the username of the user.</summary>
		[Required]
		[Display(Name = "User Name")]
		public string UserName { get; set; } = null!;

		/// <summary>Gets or sets the password of the user.</summary>
		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; } = null!;

		/// <summary>Gets or sets the password confirmation value.</summary>
		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; } = null!;
	}
}
