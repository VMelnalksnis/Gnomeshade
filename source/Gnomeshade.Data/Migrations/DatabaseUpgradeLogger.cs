// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using DbUp.Engine.Output;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Migrations;

/// <summary>Implementation of <see cref="IUpgradeLog"/> using the standard <see cref="ILogger{T}"/> interface.</summary>
/// <typeparam name="TCategoryName">The type whose name is used for the logger category name.</typeparam>
public sealed class DatabaseUpgradeLogger<TCategoryName> : IUpgradeLog
{
	private readonly ILogger<TCategoryName> _logger;

	/// <summary>Initializes a new instance of the <see cref="DatabaseUpgradeLogger{T}"/> class.</summary>
	/// <param name="logger">A generic logger for logging database upgrade logs.</param>
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
