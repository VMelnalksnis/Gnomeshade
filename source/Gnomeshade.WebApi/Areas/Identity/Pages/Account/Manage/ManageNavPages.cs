// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gnomeshade.WebApi.Areas.Identity.Pages.Account.Manage;

internal static class ManageNavPages
{
	internal static string Index => "Index";

	internal static string Email => "Email";

	internal static string ChangePassword => "ChangePassword";

	internal static string ExternalLogins => "ExternalLogins";

	internal static string PersonalData => "PersonalData";

	internal static string TwoFactorAuthentication => "TwoFactorAuthentication";

	internal static string? IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

	internal static string? EmailNavClass(ViewContext viewContext) => PageNavClass(viewContext, Email);

	internal static string? ChangePasswordNavClass(ViewContext viewContext) =>
		PageNavClass(viewContext, ChangePassword);

	internal static string? ExternalLoginsNavClass(ViewContext viewContext) =>
		PageNavClass(viewContext, ExternalLogins);

	internal static string? PersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, PersonalData);

	internal static string? TwoFactorAuthenticationNavClass(ViewContext viewContext) =>
		PageNavClass(viewContext, TwoFactorAuthentication);

	private static string? PageNavClass(ViewContext viewContext, string page)
	{
		var activePage = viewContext.ViewData["ActivePage"] as string ??
			Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);

		return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase)
			? "active"
			: null;
	}
}
