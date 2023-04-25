// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Gnomeshade.WebApi.Areas.Admin.Pages;

/// <summary>Main administration page.</summary>
public sealed class Index : PageModel
{
	/// <summary>Handles requests made by the user.</summary>
	public ActionResult OnGet()
	{
		return RedirectToPage("Users/Index", new { area = "Admin" });
	}
}
