// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Avalonia.Controls;

namespace Gnomeshade.Interfaces.Avalonia.Core.DesignTime;

/// <inheritdoc />
public sealed class DesignTimeDialogService : IDialogService
{
	/// <inheritdoc />
	public Task ShowDialog<TViewModel>(Window owner, TViewModel viewModel, Action<Window> dialogAction)
		where TViewModel : ViewModelBase
	{
		return Task.CompletedTask;
	}
}
