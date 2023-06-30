// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Avalonia.Controls.Notifications;

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
}
