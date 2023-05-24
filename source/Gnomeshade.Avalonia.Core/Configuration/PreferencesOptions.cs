// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core.Imports;

namespace Gnomeshade.Avalonia.Core.Configuration;

/// <summary>User preferences.</summary>
public sealed class PreferencesOptions
{
	/// <summary>Gets or sets the default nordigen country.</summary>
	/// <seealso cref="ImportViewModel"/>
	public string? NordigenCountry { get; set; }

	/// <summary>Gets or sets the default nordigen institution id.</summary>
	/// <seealso cref="ImportViewModel"/>
	public string? NoridgenInstitutionId { get; set; }

	/// <summary>Gets or sets the initial window height.</summary>
	/// <seealso cref="MainWindowViewModel"/>
	public int? WindowHeight { get; set; }

	/// <summary>Gets or sets the initial window width.</summary>
	/// <seealso cref="MainWindowViewModel"/>
	public int? WindowWidth { get; set; }

	/// <summary>Gets or sets the initial window state.</summary>
	/// <seealso cref="MainWindowViewModel"/>
	public WindowState? WindowState { get; set; }
}
