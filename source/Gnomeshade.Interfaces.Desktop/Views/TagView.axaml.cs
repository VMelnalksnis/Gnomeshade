// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Avalonia.Core.Tags;

namespace Gnomeshade.Interfaces.Desktop.Views;

/// <summary>An overview of of all tags.</summary>
public sealed class TagView : UserControl, IView<TagViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="TagView"/> class.</summary>
	public TagView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
