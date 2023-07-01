// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Help;

namespace Gnomeshade.Desktop.Views.Help;

/// <inheritdoc cref="AboutViewModel"/>
public sealed partial class AboutView : UserControl, IView<AboutView, AboutViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="AboutView"/> class.</summary>
	public AboutView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
