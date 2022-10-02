// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Gnomeshade.Avalonia.Core;

/// <inheritdoc cref="IActivityService"/>
public sealed class ActivityService : PropertyChangedBase, IActivityService, IDisposable
{
	private static readonly ObservableCollection<ActivityScope> _activityScopes = new();

	/// <summary>Initializes a new instance of the <see cref="ActivityService"/> class.</summary>
	public ActivityService()
	{
		_activityScopes.CollectionChanged += ValueOnCollectionChanged;
	}

	/// <inheritdoc />
	public bool IsBusy => _activityScopes.Any();

	/// <inheritdoc />
	public IDisposable BeginActivity()
	{
		var scope = new ActivityScope(_activityScopes);
		return scope;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_activityScopes.CollectionChanged -= ValueOnCollectionChanged;
	}

	private void ValueOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(IsBusy));
	}
}
