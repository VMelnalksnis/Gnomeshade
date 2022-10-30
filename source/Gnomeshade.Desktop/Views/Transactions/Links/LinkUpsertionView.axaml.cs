// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Transactions.Links;

namespace Gnomeshade.Desktop.Views.Transactions.Links;

/// <inheritdoc cref="LinkUpsertionViewModel"/>
public sealed partial class LinkUpsertionView : UserControl, IView<LinkUpsertionView, LinkUpsertionViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="LinkUpsertionView"/> class.</summary>
	public LinkUpsertionView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
