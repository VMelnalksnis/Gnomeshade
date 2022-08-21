// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Gnomeshade.Data.Migrations;

/// <summary>Migrates database to the latest version.</summary>
public interface IDatabaseMigrator
{
	/// <summary>Ensures that the database is created and upgraded to the latest version.</summary>
	void Migrate();
}
