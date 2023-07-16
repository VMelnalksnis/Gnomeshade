// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gnomeshade.Avalonia.Core.Commands;

/// <summary>Simplifies the creation of commands.</summary>
public interface ICommandFactory
{
	/// <summary>Creates a command.</summary>
	/// <param name="execute">The function that will be invoked on <see cref="ICommand.Execute"/>.</param>
	/// <param name="canExecute">The function that will be invoked on <see cref="ICommand.CanExecute"/>.</param>
	/// <param name="activity">The name of the activity for the command.</param>
	/// <returns>The command.</returns>
	CommandBase Create(Action execute, Func<bool> canExecute, string activity);

	/// <summary>Creates an async command.</summary>
	/// <param name="execute">The function that will be invoked on <see cref="ICommand.Execute"/>.</param>
	/// <param name="canExecute">The function that will be invoked on <see cref="ICommand.CanExecute"/>.</param>
	/// <param name="activity">The name of the activity for the command.</param>
	/// <returns>The command.</returns>
	CommandBase Create(Func<Task> execute, Func<bool> canExecute, string activity);

	/// <summary>Creates an async command that can always execute.</summary>
	/// <param name="execute">The function that will be invoked on <see cref="ICommand.Execute"/>.</param>
	/// <param name="activity">The name of the activity for the command.</param>
	/// <typeparam name="T">The expected type of the command parameter.</typeparam>
	/// <returns>The command.</returns>
	CommandBase Create<T>(Func<T, Task> execute, string activity);

	/// <summary>Creates an async command that can always execute.</summary>
	/// <param name="execute">The function that will be invoked on <see cref="ICommand.Execute"/>.</param>
	/// <param name="canExecute">The function that will be invoked on <see cref="ICommand.CanExecute"/>.</param>
	/// <param name="activity">The name of the activity for the command.</param>
	/// <typeparam name="T">The expected type of the command parameter.</typeparam>
	/// <returns>The command.</returns>
	CommandBase Create<T>(Func<T, Task> execute, Func<T, bool> canExecute, string activity);
}
