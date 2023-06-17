// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account;

public sealed class Logout : PageModel
{
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly ILogger<Logout> _logger;

	public Logout(SignInManager<ApplicationUser> signInManager, ILogger<Logout> logger)
	{
		_signInManager = signInManager;
		_logger = logger;
	}

	public async Task<IActionResult> OnPost(string? returnUrl = null)
	{
		await _signInManager.SignOutAsync();
		_logger.LogInformation("User logged out");
		if (returnUrl is not null)
		{
			return LocalRedirect(returnUrl);
		}

		// This needs to be a redirect so that the browser performs a new
		// request and the identity for the user gets updated.
		return RedirectToPage();
	}
}
