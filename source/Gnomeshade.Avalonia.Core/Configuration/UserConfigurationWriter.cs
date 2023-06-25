// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using static Microsoft.Extensions.Logging.LogLevel;

namespace Gnomeshade.Avalonia.Core.Configuration;

/// <summary>Persists the user configuration.</summary>
public sealed partial class UserConfigurationWriter
{
	/// <summary>Gets the filepath of the user configuration file.</summary>
	public const string Filepath = "appsettings.user.json";

	private const string _tempPath = $"{Filepath}.tmp";

	private static readonly UserConfigurationSerializationContext _context = new(new(JsonSerializerDefaults.Web)
	{
		WriteIndented = true,
		PropertyNamingPolicy = null,
	});

	private readonly ILogger<UserConfigurationWriter> _logger;
	private readonly IOptionsMonitor<UserConfiguration> _optionsMonitor;

	/// <summary>Initializes a new instance of the <see cref="UserConfigurationWriter"/> class.</summary>
	/// <param name="logger">Logger for logging in the specific category.</param>
	/// <param name="optionsMonitor">Options monitor to wait for configuration to change.</param>
	public UserConfigurationWriter(ILogger<UserConfigurationWriter> logger, IOptionsMonitor<UserConfiguration> optionsMonitor)
	{
		_logger = logger;
		_optionsMonitor = optionsMonitor;
	}

	/// <summary>Overwrites the settings stored in <see cref="Filepath"/> with <paramref name="userConfiguration"/>.</summary>
	/// <param name="userConfiguration">The new configuration values.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task WriteAsync(UserConfiguration userConfiguration)
	{
		var changed = false;
		using var change = _optionsMonitor.OnChange(_ => changed = true);

		OpeningTempFile(_tempPath);
		var fileStream = File.Open(_tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		var writer = new Utf8JsonWriter(fileStream);
		await using (fileStream)
		await using (writer)
		{
			await JsonSerializer.SerializeAsync(fileStream, userConfiguration, _context.UserConfiguration).ConfigureAwait(false);
			await fileStream.FlushAsync().ConfigureAwait(false);
		}

		WroteTempFile(_tempPath);
		if (new FileInfo(_tempPath).Length is 0)
		{
			throw new ApplicationException("Failed to save user configuration");
		}

		MovingTempFile(_tempPath, Filepath);
		File.Move(_tempPath, Filepath, true);

		var delay = TimeSpan.FromMilliseconds(100);
		while (!changed)
		{
			WaitingForUpdate();
			await Task.Delay(delay).ConfigureAwait(false);
		}

		ConfigurationUpdated();
	}

	/// <summary>Overwrites the settings stored in <see cref="Filepath"/> with <paramref name="userConfiguration"/>.</summary>
	/// <param name="userConfiguration">The new configuration values.</param>
	public void Write(UserConfiguration userConfiguration)
	{
		var changed = false;
		using var change = _optionsMonitor.OnChange(_ => changed = true);

		OpeningTempFile(_tempPath);
		var fileStream = File.Open(_tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		var writer = new Utf8JsonWriter(fileStream);
		using (fileStream)
		using (writer)
		{
			JsonSerializer.Serialize(fileStream, userConfiguration, _context.UserConfiguration);
			fileStream.Flush();
		}

		WroteTempFile(_tempPath);
		if (new FileInfo(_tempPath).Length is 0)
		{
			throw new ApplicationException("Failed to save user configuration");
		}

		MovingTempFile(_tempPath, Filepath);
		File.Move(_tempPath, Filepath, true);

		var delay = TimeSpan.FromMilliseconds(100);
		while (!changed)
		{
			WaitingForUpdate();
			Thread.Sleep(delay);
		}

		ConfigurationUpdated();
	}

	[LoggerMessage(EventId = 1, Level = Debug, Message = "Opening temporary configuration file {FilePath}")]
	private partial void OpeningTempFile(string filePath);

	[LoggerMessage(EventId = 2, Level = Debug, Message = "Wrote new configuration to temporary file {FilePath}")]
	private partial void WroteTempFile(string filePath);

	[LoggerMessage(EventId = 3, Level = Debug, Message = "Moving temporary configuration file {SourceFilePath} to {DestFilePath}")]
	private partial void MovingTempFile(string sourceFilePath, string destFilePath);

	[LoggerMessage(EventId = 4, Level = Debug, Message = "Waiting for options to read the new values")]
	private partial void WaitingForUpdate();

	[LoggerMessage(EventId = 5, Level = Information, Message = "User configuration has been saved")]
	private partial void ConfigurationUpdated();
}
