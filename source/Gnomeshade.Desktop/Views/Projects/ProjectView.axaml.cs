// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Avalonia.Core.Projects;

namespace Gnomeshade.Desktop.Views.Projects;

/// <inheritdoc cref="ProjectViewModel" />
public sealed partial class ProjectView : UserControl, IView<ProjectView, ProjectViewModel>
{
	/// <summary>Initializes a new instance of the <see cref="ProjectView"/> class.</summary>
	public ProjectView() => InitializeComponent();
}
