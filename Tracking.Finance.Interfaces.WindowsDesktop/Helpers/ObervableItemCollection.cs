// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Tracking.Finance.Interfaces.WindowsDesktop.Helpers
{
	/// <summary>
	/// Represents a dynamic data collection that provides notifications when
	/// items get added, modified, removed, or when the whole list is refreshed.
	/// </summary>
	///
	/// <typeparam name="T">The type of elements in the collection.</typeparam>
	public class ObervableItemCollection<T> : ObservableCollection<T>
		where T : INotifyPropertyChanged
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ObervableItemCollection{T}"/> class.
		/// </summary>
		public ObervableItemCollection()
		{
			CollectionChanged += Items_CollectionChanged;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObervableItemCollection{T}"/> class
		/// that contains elements copied from the specified collection.
		/// </summary>
		///
		/// <param name="collection">The collection from which the elements are copied.</param>
		public ObervableItemCollection(IEnumerable<T> collection)
			: base(collection)
		{
			CollectionChanged += Items_CollectionChanged;
			foreach (INotifyPropertyChanged item in collection)
			{
				item.PropertyChanged += Item_PropertyChanged;
			}
		}

		private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
		{
			if (eventArgs is null)
			{
				return;
			}

			if (eventArgs.OldItems is not null)
			{
				foreach (INotifyPropertyChanged item in eventArgs.OldItems)
				{
					item.PropertyChanged -= Item_PropertyChanged;
				}
			}

			if (eventArgs.NewItems is not null)
			{
				foreach (INotifyPropertyChanged item in eventArgs.NewItems)
				{
					item.PropertyChanged += Item_PropertyChanged;
				}
			}
		}

		private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs eventArgs)
		{
			var reset = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
			OnCollectionChanged(reset);
		}
	}
}
