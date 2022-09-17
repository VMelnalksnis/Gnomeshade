// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace Gnomeshade.Avalonia.Core.Configuration;

/// <summary>Persists the user configuration.</summary>
public sealed class UserConfigurationWriter
{
	/// <summary>Gets the filepath of the user configuration file.</summary>
	public const string Filepath = "appsettings.user.json";

	private static readonly UserConfigurationSerializationContext _context = new(new(JsonSerializerDefaults.Web)
	{
		WriteIndented = true,
		PropertyNamingPolicy = null,
	});

	private readonly IOptionsMonitor<UserConfiguration> _optionsMonitor;

	/// <summary>Initializes a new instance of the <see cref="UserConfigurationWriter"/> class.</summary>
	/// <param name="optionsMonitor">Options monitor to wait for configuration to change.</param>
	public UserConfigurationWriter(IOptionsMonitor<UserConfiguration> optionsMonitor)
	{
		_optionsMonitor = optionsMonitor;
	}

	/// <summary>Overwrites the settings stored in <see cref="Filepath"/> with <paramref name="userConfiguration"/>.</summary>
	/// <param name="userConfiguration">The new configuration values.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task Write(UserConfiguration userConfiguration)
	{
		var changed = false;
		_optionsMonitor.OnChange(_ => changed = true);

		var fileStream = File.Open(Filepath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
		var writer = new Utf8JsonWriter(fileStream);
		await using (fileStream)
		await using (writer)
		{
			await JsonSerializer.SerializeAsync(fileStream, userConfiguration, _context.UserConfiguration);
		}

		var delay = TimeSpan.FromMilliseconds(100);
		while (!changed)
		{
			await Task.Delay(delay);
		}
	}
}
