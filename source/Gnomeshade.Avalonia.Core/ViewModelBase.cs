// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Base class for all view models.</summary>
public abstract class ViewModelBase : PropertyChangedBase, IDisposable
{
	private bool _disposed;

	/// <summary>Initializes a new instance of the <see cref="ViewModelBase"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	protected ViewModelBase(IActivityService activityService)
	{
		ActivityService = activityService;
		ActivityService.PropertyChanged += ActivityServiceOnPropertyChanged;
	}

	/// <summary>Gets a value indicating whether the viewmodel busy.</summary>
	public bool IsBusy => ActivityService.IsBusy;

	/// <summary>Gets the name of all current activities.</summary>
	public string ActivityName => string.Join(", ", ActivityService.Activities);

	/// <summary>Gets a service for managing application activity indicators.</summary>
	protected IActivityService ActivityService { get; }

	/// <summary>Refreshes all data loaded from API.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task RefreshAsync()
	{
		using var activity = BeginActivity("Refreshing");

		try
		{
			await Refresh();
		}
		catch (Exception exception)
		{
			ActivityService.ShowErrorNotification(exception.Message);
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <param name="name"></param>
	/// <inheritdoc cref="IActivityService.BeginActivity"/>
	protected IDisposable BeginActivity(string name) => ActivityService.BeginActivity(name);

	/// <summary>Refreshes all data loaded from API.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	protected virtual Task Refresh() => Task.CompletedTask;

	/// <summary>Releases and/ord resets any resources, such as event handlers.</summary>
	/// <param name="disposing"><c>true</c> when called from <see cref="IDisposable.Dispose"/>, or <c>false</c> when called from finalizer.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}

		if (disposing)
		{
			ActivityService.PropertyChanged -= ActivityServiceOnPropertyChanged;
		}

		_disposed = true;
	}

	private void ActivityServiceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(IActivityService.IsBusy))
		{
			OnPropertyChanged(nameof(IsBusy));
		}

		if (e.PropertyName is nameof(IActivityService.Activities))
		{
			OnPropertyChanged(nameof(ActivityName));
		}
	}
}
