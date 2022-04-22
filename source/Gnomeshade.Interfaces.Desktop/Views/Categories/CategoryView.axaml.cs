// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Avalonia.Core.Categories;

namespace Gnomeshade.Interfaces.Desktop.Views.Categories;

/// <summary>An overview of of all categories.</summary>
public sealed class CategoryView : UserControl, IView<CategoryViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="CategoryView"/> class.</summary>
	public CategoryView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
