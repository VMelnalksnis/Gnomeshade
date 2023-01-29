// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Gnomeshade.Data;
using Gnomeshade.Data.Identity;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account;

/// <summary>Page for handling new user registration.</summary>
[AllowAnonymous]
[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1615:Element return value should be documented", Justification = "No reason to document IActionResult")]
public sealed class Register : PageModel
{
	private readonly ILogger<Register> _logger;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IEmailSender _emailSender;
	private readonly UserUnitOfWork _userUnitOfWork;

	/// <summary>Initializes a new instance of the <see cref="Register"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="signInManager">Application user sign in manager.</param>
	/// <param name="userManager">Application user manager.</param>
	/// <param name="emailSender">Registration confirmation email sender.</param>
	/// <param name="userUnitOfWork">Application user persistence store.</param>
	public Register(
		ILogger<Register> logger,
		SignInManager<ApplicationUser> signInManager,
		UserManager<ApplicationUser> userManager,
		IEmailSender emailSender,
		UserUnitOfWork userUnitOfWork)
	{
		_logger = logger;
		_signInManager = signInManager;
		_userManager = userManager;
		_emailSender = emailSender;
		_userUnitOfWork = userUnitOfWork;
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
			var user = new ApplicationUser
			{
				Email = Input.Email,
				FullName = Input.FullName,
				UserName = Input.UserName,
			};

			var result = await _userManager.CreateAsync(user, Input.Password);

			if (result.Succeeded)
			{
				_logger.LogInformation("User created a new account with password");

				var identityUser = await _userManager.FindByNameAsync(user.UserName);
				if (identityUser is null)
				{
					throw new InvalidOperationException("Could not find user by name after creating it");
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

				var userId = await _userManager.GetUserIdAsync(user);
				var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
				code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
				var callbackUrl = Url.Page(
					"/Account/ConfirmEmail",
					pageHandler: null,
					values: new { area = "Identity", userId, code, returnUrl },
					protocol: Request.Scheme);

				if (callbackUrl is null)
				{
					throw new InvalidOperationException("Expected callback url to have a value");
				}

				await _emailSender.SendEmailAsync(
					Input.Email,
					"Confirm your email",
					$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

				if (_userManager.Options.SignIn.RequireConfirmedAccount)
				{
					return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
				}

				await _signInManager.SignInAsync(user, isPersistent: false);
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
	[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
	public sealed class InputModel
	{
		/// <summary>Gets or sets the email of the user.</summary>
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; } = null!;

		/// <summary>Gets or sets the full name of the user.</summary>
		[Required]
		[Display(Name = "Full Name")]
		public string FullName { get; set; } = null!;

		/// <summary>Gets or sets the user name of the user.</summary>
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
		[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Unused otherProperty will have it's own warning")]
		public string ConfirmPassword { get; set; } = null!;
	}
}
