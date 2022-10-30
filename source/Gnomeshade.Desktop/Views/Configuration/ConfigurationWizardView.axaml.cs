// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Configuration;

namespace Gnomeshade.Desktop.Views.Configuration;

/// <inheritdoc cref="ConfigurationWizardViewModel"/>
public sealed partial class ConfigurationWizardView : UserControl, IView<ConfigurationWizardView, ConfigurationWizardViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="ConfigurationWizardView"/> class.</summary>
	public ConfigurationWizardView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
