// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Npgsql.Logging;

using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace Gnomeshade.Interfaces.WebApi.Logging;

/// <summary>A <see cref="ILogger"/> wrapper for Npgsql.</summary>
public sealed class SerilogNpgsqlLogger : NpgsqlLogger
{
	private readonly ILogger _logger;

	/// <summary>Initializes a new instance of the <see cref="SerilogNpgsqlLogger"/> class.</summary>
	/// <param name="logger">The logger to use for logging.</param>
	public SerilogNpgsqlLogger(ILogger logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public override bool IsEnabled(NpgsqlLogLevel level)
	{
		var eventLevel = Translate(level);
		return _logger.IsEnabled(eventLevel);
	}

	/// <inheritdoc />
	public override void Log(NpgsqlLogLevel level, int connectorId, string msg, Exception? exception = null)
	{
		using var pushProperty = LogContext.PushProperty("NpgsqlConnectorId", connectorId.ToString());
		var eventLevel = Translate(level);

		// This is a wrapper, so message cannot be controlled
		// ReSharper disable once TemplateIsNotCompileTimeConstantProblem
		_logger.Write(eventLevel, exception, msg);
	}

	private static LogEventLevel Translate(NpgsqlLogLevel logLevel) => logLevel switch
	{
		NpgsqlLogLevel.Fatal => LogEventLevel.Fatal,
		NpgsqlLogLevel.Error => LogEventLevel.Error,
		NpgsqlLogLevel.Warn => LogEventLevel.Warning,
		NpgsqlLogLevel.Info => LogEventLevel.Information,
		NpgsqlLogLevel.Debug => LogEventLevel.Debug,
		NpgsqlLogLevel.Trace => LogEventLevel.Verbose,
		_ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, $"Unexpected {nameof(NpgsqlLogLevel)}"),
	};
}
