// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Diagnostics;

using Microsoft.Extensions.Configuration;

using Serilog;

namespace Gnomeshade.Desktop;

internal static class SerilogConfiguration
{
	private const string _boostrapLogPath = "bootstrap.log";
	private const string _applicationLogPath = "application.log";
	private const int _fileSizeLimit = 10 * 1024 * 1024;
	private const int _fileCountLimit = 2;

	internal static void InitializeBootstrapLogger()
	{
		Serilog.Debugging.SelfLog.Enable(output => Debug.WriteLine(output));

		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Verbose()
			.Enrich.FromLogContext()
			.WriteTo.Trace()
			.WriteTo.File(
				_boostrapLogPath,
				shared: true,
				fileSizeLimitBytes: _fileSizeLimit,
				rollOnFileSizeLimit: true,
				retainedFileCountLimit: _fileCountLimit)
			.CreateBootstrapLogger();
	}

	internal static ILogger CreateLogger(IConfiguration configuration) => new LoggerConfiguration()
		.MinimumLevel.Information()
		.ReadFrom.Configuration(configuration)
		.Enrich.FromLogContext()
		.WriteTo.File(
			_applicationLogPath,
			fileSizeLimitBytes: _fileSizeLimit,
			rollOnFileSizeLimit: true,
			retainedFileCountLimit: _fileCountLimit)
		.CreateLogger();
}
