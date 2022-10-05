// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gnomeshade.WebApi.Controllers;

/// <summary>The home page and shared views.</summary>
[AllowAnonymous]
[Route("[controller]/[action]")]
public sealed class HomeController : Controller
{
	/// <summary>Gets the home page.</summary>
	/// <returns>The home page.</returns>
	[HttpGet("/")]
	public ViewResult Index() => View();
}
