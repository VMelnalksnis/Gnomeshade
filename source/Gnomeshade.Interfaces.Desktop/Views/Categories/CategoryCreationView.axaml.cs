﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Avalonia.Core.Categories;

namespace Gnomeshade.Interfaces.Desktop.Views.Categories;

/// <summary>Create or update single category.</summary>
public sealed class CategoryCreationView : UserControl, IView<CategoryCreationViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="CategoryCreationView"/> class.</summary>
	public CategoryCreationView()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
