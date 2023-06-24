// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Avalonia.Controls.Notifications;

namespace Gnomeshade.Avalonia.Core;

/// <inheritdoc cref="IActivityService"/>
public sealed class ActivityService : PropertyChangedBase, IActivityService, IDisposable
{
	private static readonly ObservableCollection<ActivityScope> _activityScopes = new();

	// This needs to be lazy, because this needs to be created before MainWindow to create its DataContext
	private readonly Lazy<IManagedNotificationManager> _lazyNotificationManager;

	/// <summary>Initializes a new instance of the <see cref="ActivityService"/> class.</summary>
	/// <param name="notificationManager">Used for displaying notifications.</param>
	public ActivityService(Lazy<IManagedNotificationManager> notificationManager)
	{
		_lazyNotificationManager = notificationManager;
		_activityScopes.CollectionChanged += ValueOnCollectionChanged;
	}

	/// <inheritdoc />
	public bool IsBusy => _activityScopes.Any();

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
	public void Dispose() => _activityScopes.CollectionChanged -= ValueOnCollectionChanged;

	private void ValueOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(IsBusy));
		OnPropertyChanged(nameof(Activities));
	}
}
