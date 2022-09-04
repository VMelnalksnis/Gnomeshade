// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Ardalis.SmartEnum;

namespace Gnomeshade.WebApi.Configuration.Options;

internal sealed class DatabaseProvider : SmartEnum<DatabaseProvider>
{
	public static readonly DatabaseProvider PostgreSQL = new(nameof(PostgreSQL), 1);
	public static readonly DatabaseProvider Sqlite = new(nameof(Sqlite), 2);

	private DatabaseProvider(string name, int value)
		: base(name, value)
	{
	}
}
