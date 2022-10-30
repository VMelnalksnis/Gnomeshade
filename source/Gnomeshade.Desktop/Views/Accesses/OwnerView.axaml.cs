// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Accesses;

namespace Gnomeshade.Desktop.Views.Accesses;

/// <inheritdoc cref="OwnerViewModel"/>
public sealed partial class OwnerView : UserControl, IView<OwnerView, OwnerViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="OwnerView"/> class.</summary>
	public OwnerView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
