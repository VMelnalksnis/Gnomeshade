// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using DbUp.Engine.Output;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Migrations;

internal sealed class DatabaseUpgradeLogger<TCategoryName> : IUpgradeLog
{
	private readonly ILogger<TCategoryName> _logger;

	public DatabaseUpgradeLogger(ILogger<TCategoryName> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public void WriteInformation(string format, params object[] args) => _logger.LogInformation(format, args);

	/// <inheritdoc />
	public void WriteError(string format, params object[] args) => _logger.LogError(format, args);

	/// <inheritdoc />
	public void WriteWarning(string format, params object[] args) => _logger.LogWarning(format, args);
}
