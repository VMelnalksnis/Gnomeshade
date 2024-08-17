// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls.Notifications;

using Gnomeshade.Avalonia.Core.Commands;

using Microsoft.Extensions.Logging.Abstractions;

namespace Gnomeshade.Avalonia.Core.Tests;

internal sealed class StubbedActivityService : PropertyChangedBase, IActivityService
{
	private readonly ActivityService _activityService;

	public StubbedActivityService()
	{
		_activityService = new(
			new(() => new WindowNotificationManager()),
			NullLogger<ActivityService>.Instance,
			NullLoggerFactory.Instance);
	}

	public bool IsBusy => _activityService.IsBusy;

	public IEnumerable<string> Activities => _activityService.Activities;

	public IDisposable BeginActivity(string name) => _activityService.BeginActivity(name);

	public void ShowNotification(Notification notification) => _activityService.ShowNotification(notification);

	public void ShowErrorNotification(string message) => _activityService.ShowErrorNotification(message);

	public void ShowErrorNotification(Exception exception) => _activityService.ShowErrorNotification(exception);

	public CommandBase Create(Action execute, Func<bool> canExecute, string activity) =>
		_activityService.Create(execute, canExecute, activity);

	public CommandBase Create(Func<Task> execute, string activity) => _activityService.Create(execute, activity);

	public CommandBase Create(Func<Task> execute, Func<bool> canExecute, string activity) =>
		_activityService.Create(execute, canExecute, activity);

	public CommandBase Create<T>(Func<T, Task> execute, string activity) => _activityService.Create(execute, activity);

	public CommandBase Create<T>(Func<T, Task> execute, Func<T, bool> canExecute, string activity) =>
		_activityService.Create(execute, canExecute, activity);
}
