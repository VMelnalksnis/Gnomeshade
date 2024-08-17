// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls.Notifications;

using Gnomeshade.Avalonia.Core.Commands;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Avalonia.Core;

/// <inheritdoc cref="IActivityService"/>
public sealed class ActivityService : PropertyChangedBase, IActivityService, IDisposable
{
	private readonly ObservableCollection<ActivityScope> _activityScopes = [];

	// This needs to be lazy, because this needs to be created before MainWindow to create its DataContext
	private readonly Lazy<IManagedNotificationManager> _lazyNotificationManager;
	private readonly ILogger<ActivityService> _logger;
	private readonly ILoggerFactory _loggerFactory;

	/// <summary>Initializes a new instance of the <see cref="ActivityService"/> class.</summary>
	/// <param name="notificationManager">Used for displaying notifications.</param>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="loggerFactory">Logger factory for creating loggers for each command.</param>
	public ActivityService(
		Lazy<IManagedNotificationManager> notificationManager,
		ILogger<ActivityService> logger,
		ILoggerFactory loggerFactory)
	{
		_lazyNotificationManager = notificationManager;
		_logger = logger;
		_loggerFactory = loggerFactory;
		_activityScopes.CollectionChanged += ValueOnCollectionChanged;
	}

	/// <inheritdoc />
	public bool IsBusy => _activityScopes.Count is not 0;

	/// <inheritdoc />
	public IEnumerable<string> Activities => _activityScopes
		.Select(scope => scope.Name)
		.Distinct(StringComparer.OrdinalIgnoreCase);

	/// <inheritdoc />
	public IDisposable BeginActivity(string name)
	{
		var scope = new ActivityScope(_activityScopes, name);
		return scope;
	}

	/// <inheritdoc />
	public void ShowNotification(Notification notification) => _lazyNotificationManager.Value.Show(notification);

	/// <inheritdoc />
	public void ShowErrorNotification(string message) =>
		ShowNotification(new("Unexpected error", message, NotificationType.Error));

	/// <inheritdoc />
	public void ShowErrorNotification(Exception exception)
	{
		_logger.LogWarning(exception, "Unexpected error");
		ShowErrorNotification(exception.Message);
	}

	/// <inheritdoc />
	public CommandBase Create(Action execute, Func<bool> canExecute, string activity)
	{
		var logger = _loggerFactory.CreateLogger<Command>();
		return new Command(logger, this, execute, canExecute, activity);
	}

	/// <inheritdoc />
	public CommandBase Create(Func<Task> execute, string activity) => Create(execute, static () => true, activity);

	/// <inheritdoc />
	public CommandBase Create(Func<Task> execute, Func<bool> canExecute, string activity)
	{
		var logger = _loggerFactory.CreateLogger<AsyncCommand>();
		return new AsyncCommand(logger, this, execute, canExecute, activity);
	}

	/// <inheritdoc />
	public CommandBase Create<T>(Func<T, Task> execute, string activity) => Create(execute, static _ => true, activity);

	/// <inheritdoc />
	public CommandBase Create<T>(Func<T, Task> execute, Func<T, bool> canExecute, string activity)
	{
		var logger = _loggerFactory.CreateLogger<AsyncCommand<T>>();
		return new AsyncCommand<T>(logger, this, execute, canExecute, activity);
	}

	/// <inheritdoc />
	public void Dispose() => _activityScopes.CollectionChanged -= ValueOnCollectionChanged;

	private void ValueOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(IsBusy));
		OnPropertyChanged(nameof(Activities));
	}
}
