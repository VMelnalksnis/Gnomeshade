// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using static System.StringSplitOptions;

namespace Gnomeshade.Data.Migrations;

/// <summary>Compares migration scripts by file name ignoring namespaces.</summary>
internal sealed class MigrationScriptNameComparer : IComparer<string?>
{
	/// <inheritdoc />
	public int Compare(string? x, string? y)
	{
		var firstName = GetFileName(x);
		var secondName = GetFileName(y);

		return StringComparer.OrdinalIgnoreCase.Compare(firstName, secondName);
	}

	private static string? GetFileName(string? filepath)
	{
		var parts = filepath?.Split('.', RemoveEmptyEntries | TrimEntries);
		return parts?.Skip(parts.Length - 2).First();
	}
}
