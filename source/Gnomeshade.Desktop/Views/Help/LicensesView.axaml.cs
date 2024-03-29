﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Help;

namespace Gnomeshade.Desktop.Views.Help;

/// <inheritdoc cref="LicensesViewModel"/>
public sealed partial class LicensesView : UserControl, IView<LicensesView, LicensesViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="LicensesView"/> class.</summary>
	public LicensesView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
