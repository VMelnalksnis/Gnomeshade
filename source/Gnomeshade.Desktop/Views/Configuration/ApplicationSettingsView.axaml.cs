﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Configuration;

namespace Gnomeshade.Desktop.Views.Configuration;

/// <inheritdoc cref="ApplicationSettingsViewModel"/>
public sealed partial class ApplicationSettingsView : UserControl, IView<ApplicationSettingsView, ApplicationSettingsViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="ApplicationSettingsView"/> class.</summary>
	public ApplicationSettingsView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
