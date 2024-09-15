// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Projects;

namespace Gnomeshade.Desktop.Views.Projects;

/// <inheritdoc cref="ProjectUpsertionViewModel"/>
public sealed partial class ProjectUpsertionView : UserControl, IView<ProjectUpsertionView, ProjectUpsertionViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="ProjectUpsertionView"/> class.</summary>
	public ProjectUpsertionView() => AvaloniaXamlLoader.Load(this);
}
