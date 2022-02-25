// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Desktop.ViewModels;

namespace Gnomeshade.Interfaces.Desktop.Views;

public sealed class UnitCreationView : FocusOnInitUserControl, IView<UnitCreationViewModel>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UnitCreationView"/> class.
	/// </summary>
	public UnitCreationView()
	{
		AvaloniaXamlLoader.Load(this);
		Focus("UnitName");
	}
}
