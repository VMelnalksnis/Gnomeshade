// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Avalonia.Collections;

namespace Gnomeshade.Interfaces.Avalonia.Core;

/// <summary>Base class for view models which display all available entities of a specific type.</summary>
/// <typeparam name="TRow">The type of items displayed in data grid.</typeparam>
public abstract class OverviewViewModel<TRow> : ViewModelBase
	where TRow : PropertyChangedBase
{
	private static readonly string[] _dataGridViewNames = { nameof(DataGridView) };

	private DataGridItemCollectionView<TRow> _rows = new(Array.Empty<TRow>());
	private TRow? _selected;

	/// <summary>Gets the grid view of all <see cref="Rows"/>.</summary>
	public DataGridCollectionView DataGridView => Rows;

	/// <summary>Gets or sets a typed collection of items in <see cref="DataGridView"/>.</summary>
	public DataGridItemCollectionView<TRow> Rows
	{
		get => _rows;
		protected set => SetAndNotifyWithGuard(ref _rows, value, nameof(Rows), _dataGridViewNames);
	}

	/// <summary>Gets or sets the select row from <see cref="Rows"/>.</summary>
	public TRow? Selected
	{
		get => _selected;
		set => SetAndNotify(ref _selected, value);
	}
}
