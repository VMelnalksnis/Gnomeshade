// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.WebApi.V1.Importing.Paperless.Rimi;

internal static class Constants
{
	internal const StringComparison Comparison = StringComparison.OrdinalIgnoreCase;

	internal static readonly string[] DiscountIdentifiers =
	{
		"Atl. ",
		"Atl ",
		"ati ",
	};
}
