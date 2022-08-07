// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using Avalonia.Collections;
using Avalonia.Data;

namespace Gnomeshade.Avalonia.Core;

/// <summary>
/// Strongly typed wrapper for <see cref="DataGridCollectionView"/>,
/// which notifies when items in the collection have changed.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
/// <remarks>
/// <see cref="DataGridCollectionView"/> needs to be used so that <see cref="BindingMode.TwoWay"/> binding works for the items.
/// Furthermore, <see cref="DataGridCollectionView"/> does not notify of changes made to items.
/// </remarks>
public sealed class DataGridItemCollectionView<T> : IEnumerable<T>, INotifyCollectionChanged
	where T : class, INotifyPropertyChanged
{
	private readonly DataGridCollectionView _dataGridCollectionView;

	/// <summary>
	/// Initializes a new instance of the <see cref="DataGridItemCollectionView{T}"/> class from a collection of items.
	/// </summary>
	/// <param name="source">The source for the collection.</param>
	public DataGridItemCollectionView(IEnumerable<T> source)
		: this(new DataGridCollectionView(source))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DataGridItemCollectionView{T}"/> class from a <see cref="DataGridCollectionView"/>.
	/// </summary>
	/// <param name="dataGridCollectionView">The data grid view to wrap.</param>
	public DataGridItemCollectionView(DataGridCollectionView dataGridCollectionView)
	{
		_dataGridCollectionView = dataGridCollectionView;
		_dataGridCollectionView.CollectionChanged += DataGridCollectionViewOnCollectionChanged;
		foreach (INotifyPropertyChanged item in dataGridCollectionView)
		{
			item.PropertyChanged += ItemOnPropertyChanged;
		}
	}

	/// <summary>
	/// Occurs when the underlying <see cref="DataGridCollectionView.CollectionChanged"/> is raised,
	/// or when <see cref="INotifyPropertyChanged.PropertyChanged"/> is raised by one of the items.
	/// </summary>
	public event NotifyCollectionChangedEventHandler? CollectionChanged;

	/// <summary>
	/// Implicitly converts a <see cref="DataGridItemCollectionView{T}"/> to the underlying <see cref="DataGridCollectionView"/>.
	/// </summary>
	/// <param name="view">The collection view to convert.</param>
	/// <returns>The underlying <see cref="DataGridCollectionView"/>.</returns>
	public static implicit operator DataGridCollectionView(DataGridItemCollectionView<T> view)
	{
		return view._dataGridCollectionView;
	}

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator()
	{
		return _dataGridCollectionView.Cast<T>().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() => _dataGridCollectionView.GetEnumerator();

	private void DataGridCollectionViewOnCollectionChanged(
		object? sender,
		NotifyCollectionChangedEventArgs eventArgs)
	{
		if (eventArgs.OldItems is not null)
		{
			foreach (INotifyPropertyChanged oldItem in eventArgs.OldItems)
			{
				oldItem.PropertyChanged -= ItemOnPropertyChanged;
			}
		}

		if (eventArgs.NewItems is not null)
		{
			foreach (INotifyPropertyChanged newItem in eventArgs.NewItems)
			{
				newItem.PropertyChanged += ItemOnPropertyChanged;
			}
		}

		CollectionChanged?.Invoke(this, eventArgs);
	}

	private void ItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		DataGridCollectionViewOnCollectionChanged(this, new(NotifyCollectionChangedAction.Reset));
	}
}
