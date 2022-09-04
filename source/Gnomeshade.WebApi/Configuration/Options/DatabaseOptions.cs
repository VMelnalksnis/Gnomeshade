// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Gnomeshade.WebApi.Configuration.Options;

/// <summary>Options relating to databases, except for connection string.</summary>
public sealed class DatabaseOptions
{
	/// <summary>Gets the name of the <see cref="DatabaseProvider"/>.</summary>
	public string Provider { get; init; } = DatabaseProvider.PostgreSQL.Name;
}
