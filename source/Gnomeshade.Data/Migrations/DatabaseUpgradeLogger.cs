// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using DbUp.Engine.Output;

using Microsoft.Extensions.Logging;

// ReSharper disable ContextualLoggerProblem
// ReSharper disable TemplateIsNotCompileTimeConstantProblem
#pragma warning disable CA2254

namespace Gnomeshade.Data.Migrations;

internal sealed class DatabaseUpgradeLogger<TCategoryName> : IUpgradeLog
{
	private readonly ILogger<TCategoryName> _logger;

	public DatabaseUpgradeLogger(ILogger<TCategoryName> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public void LogTrace(string format, params object[] args) => Log(LogLevel.Trace, format, args);

	/// <inheritdoc />
	public void LogDebug(string format, params object[] args) => Log(LogLevel.Debug, format, args);

	/// <inheritdoc />
	public void LogInformation(string format, params object[] args) => Log(LogLevel.Information, format, args);

	/// <inheritdoc />
	public void LogWarning(string format, params object[] args) => Log(LogLevel.Warning, format, args);

	/// <inheritdoc />
	public void LogError(string format, params object[] args) => Log(LogLevel.Error, format, args);

	/// <inheritdoc />
	public void LogError(Exception ex, string format, params object[] args) => Log(LogLevel.Error, format, args, ex);

	private void Log(LogLevel level, string format, object[] args, Exception? exception = null)
	{
		_logger.Log(level, exception, format, args);
	}
}
