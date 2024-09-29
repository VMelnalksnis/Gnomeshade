// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

#if DEBUG
using System.Diagnostics.CodeAnalysis;
#endif

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;

namespace Gnomeshade.Desktop.Views;

/// <summary>Used by <see cref="ViewLocator{TAssembly}"/> when could not locate a view in debug mode.</summary>
#if DEBUG
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
public sealed partial class PlaceholderView : UserControl
{
	/// <summary>Initializes a new instance of the <see cref="PlaceholderView"/> class.</summary>
	public PlaceholderView() => InitializeComponent();
}
