// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Npgsql.Logging;

using Serilog.Core;

namespace Gnomeshade.Interfaces.WebApi.Logging;

/// <summary>
/// Provides <see cref="SerilogNpgsqlLogger"/> for Npgsql.
/// </summary>
public sealed class SerilogNpgsqlLoggingProvider : INpgsqlLoggingProvider
{
	/// <inheritdoc />
	public NpgsqlLogger CreateLogger(string name)
	{
		var logger = Serilog.Log.ForContext(Constants.SourceContextPropertyName, name);
		return new SerilogNpgsqlLogger(logger);
	}
}
