// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Windows.Input;

namespace Gnomeshade.Avalonia.Core.Commands;

/// <inheritdoc />
public abstract class CommandBase : ICommand
{
	/// <inheritdoc />
	public event EventHandler? CanExecuteChanged;

	/// <inheritdoc />
	public abstract bool CanExecute(object? parameter);

	/// <inheritdoc />
	public abstract void Execute(object? parameter);

	/// <summary>Invokes <see cref="CanExecuteChanged"/>.</summary>
	public void InvokeExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
