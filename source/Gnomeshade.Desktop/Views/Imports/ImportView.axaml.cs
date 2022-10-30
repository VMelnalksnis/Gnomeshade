// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Imports;

namespace Gnomeshade.Desktop.Views.Imports;

/// <inheritdoc cref="ImportViewModel"/>
public sealed partial class ImportView : UserControl, IView<ImportView, ImportViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="ImportView"/> class.</summary>
	public ImportView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
