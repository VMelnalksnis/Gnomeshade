// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using DbUp.Engine;

namespace Gnomeshade.WebApi.Tests.Integration.Fixtures;

/// <inheritdoc />
/// <seealso href="https://www.postgresql.org/docs/current/sql-createtable.html#SQL-CREATETABLE-UNLOGGED"/>
internal sealed class UnloggedTableScriptPreprocessor : IScriptPreprocessor
{
	/// <inheritdoc />
	public string Process(string contents) => contents.Replace("CREATE TABLE", "CREATE UNLOGGED TABLE");
}
