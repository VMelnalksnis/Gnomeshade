﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Markup.Xaml;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Avalonia.Core.Tags;

namespace Gnomeshade.Interfaces.Desktop.Views;

/// <summary>Create or update single tag.</summary>
public sealed class TagCreationView : FocusOnInitUserControl, IView<TagCreationViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="TagCreationView"/> class.</summary>
	public TagCreationView()
	{
		AvaloniaXamlLoader.Load(this);
		Focus("TagName");
	}
}
