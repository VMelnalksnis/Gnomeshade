// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account.Manage;

public sealed class DownloadPersonalData : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ILogger<DownloadPersonalData> _logger;

	public DownloadPersonalData(UserManager<ApplicationUser> userManager, ILogger<DownloadPersonalData> logger)
	{
		_userManager = userManager;
		_logger = logger;
	}

	public IActionResult OnGet() => NotFound();

	public async Task<IActionResult> OnPostAsync()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user is null)
		{
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
		}

		_logger.UserRequestedPersonalData(user.Id);

		// Only include personal data for download
		var personalData = new Dictionary<string, string>();
		var personalDataProps = typeof(ApplicationUser)
			.GetProperties()
			.Where(property => Attribute.IsDefined(property, typeof(PersonalDataAttribute)));

		const string defaultValue = "null";
		foreach (var property in personalDataProps)
		{
			personalData.Add(property.Name, property.GetValue(user)?.ToString() ?? defaultValue);
		}

		var logins = await _userManager.GetLoginsAsync(user);
		foreach (var login in logins)
		{
			personalData.Add($"{login.LoginProvider} external login provider key", login.ProviderKey);
		}

		personalData.Add("Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user) ?? defaultValue);

		Response.Headers["Content-Disposition"] = "attachment; filename=PersonalData.json";
		return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
	}
}
