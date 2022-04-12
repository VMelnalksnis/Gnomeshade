﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Links;

namespace Gnomeshade.Interfaces.Desktop.Views.Transactions.Links;

/// <inheritdoc cref="LinkViewModel" />
public sealed class LinkView : UserControl, IView<LinkViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="LinkView"/> class.</summary>
	public LinkView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
