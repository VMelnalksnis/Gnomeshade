// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

using Avalonia.Controls.Notifications;

using Gnomeshade.Avalonia.Core.Commands;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Service for indicating the activity of the application to the user.</summary>
public interface IActivityService : INotifyPropertyChanged
{
	/// <summary>Gets a value indicating whether the application is busy.</summary>
	bool IsBusy { get; }

	/// <summary>Gets all current activities.</summary>
	public IEnumerable<string> Activities { get; }

	/// <summary>Creates a new <see cref="IDisposable"/> whose lifetime represents the activity lifetime.</summary>
	/// <param name="name">The name of the activity.</param>
	/// <returns>An <see cref="IDisposable"/> whose lifetime represents the lifetime of the activity.</returns>
	IDisposable BeginActivity(string name);

	/// <summary>Shows a notification.</summary>
	/// <param name="notification">The notification to show.</param>
	void ShowNotification(Notification notification);

	/// <summary>Shows a notification of an error.</summary>
	/// <param name="message">Details about the error.</param>
	void ShowErrorNotification(string message);

	/// <summary>Shows a notification of an error.</summary>
	/// <param name="exception">Details about the error.</param>
	void ShowErrorNotification(Exception exception);

	/// <summary>Creates a command.</summary>
	/// <param name="execute">The function that will be invoked on <see cref="ICommand.Execute"/>.</param>
	/// <param name="canExecute">The function that will be invoked on <see cref="ICommand.CanExecute"/>.</param>
	/// <param name="activity">The name of the activity for the command.</param>
	/// <returns>The command.</returns>
	CommandBase Create(Action execute, Func<bool> canExecute, string activity);

	/// <summary>Creates an async command that can always execute.</summary>
	/// <param name="execute">The function that will be invoked on <see cref="ICommand.Execute"/>.</param>
	/// <param name="activity">The name of the activity for the command.</param>
	/// <returns>The command.</returns>
	CommandBase Create(Func<Task> execute, string activity);

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
