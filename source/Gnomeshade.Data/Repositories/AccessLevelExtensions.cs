// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Gnomeshade.Data.Repositories;

internal static class AccessLevelExtensions
{
	internal static string? ToParam(this AccessLevel accessLevel) => accessLevel switch
	{
		AccessLevel.Read => "READ",
		AccessLevel.Write => "WRITE",
		AccessLevel.Delete => "DELETE",
		_ => null,
	};
}
