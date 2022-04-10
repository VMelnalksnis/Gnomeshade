// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Imports;

/// <summary>External data import view model.</summary>
public sealed class ImportViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private string? _filePath;

	/// <summary>Initializes a new instance of the <see cref="ImportViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	public ImportViewModel(IGnomeshadeClient gnomeshadeClient)
	{
		_gnomeshadeClient = gnomeshadeClient;
	}

	/// <summary>Gets or sets the local path of the report file to import.</summary>
	public string? FilePath
	{
		get => _filePath;
		set => SetAndNotifyWithGuard(ref _filePath, value, nameof(FilePath));
	}

	/// <summary>Gets a value indicating whether the information needed for <see cref="ImportAsync"/> is valid.</summary>
	public bool CanImport => !string.IsNullOrWhiteSpace(FilePath);

	/// <summary>Imports the located at <see cref="FilePath"/>.</summary>
	/// <exception cref="InvalidOperationException"><see cref="FilePath"/> is null or whitespace.</exception>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task ImportAsync()
	{
		if (string.IsNullOrWhiteSpace(FilePath))
		{
			throw new InvalidOperationException($"Cannot import file because {nameof(FilePath)} is null");
		}

		var file = new FileInfo(FilePath);
		await using var stream = file.OpenRead();
		await _gnomeshadeClient.Import(stream, file.Name);
	}
}
