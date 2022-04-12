﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Avalonia.Collections;

namespace Gnomeshade.Interfaces.Avalonia.Core;

/// <summary>Base class for view models which display all available entities of a specific type.</summary>
/// <typeparam name="TRow">The type of items displayed in data grid.</typeparam>
/// <typeparam name="TUpsertion">The view model type for creating or updating rows.</typeparam>
[SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "Better than allocating per call")]
public abstract class OverviewViewModel<TRow, TUpsertion> : ViewModelBase
	where TRow : PropertyChangedBase
	where TUpsertion : UpsertionViewModel?
{
	private static readonly string[] _dataGridViewNames = { nameof(DataGridView) };
	private static readonly string[] _selectedGuardNames = { nameof(Details), nameof(CanDelete) };

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
		set => SetAndNotifyWithGuard(ref _selected, value, nameof(Selected), _selectedGuardNames);
	}

	/// <summary>Gets or sets the view model for the currently <see cref="Selected"/> row.</summary>
	public abstract TUpsertion Details { get; set; }

	/// <summary>Gets a value indicating whether <see cref="DeleteSelectedAsync"/> can be called.</summary>
	public bool CanDelete => Selected is not null;

	/// <summary>Deletes <see cref="Selected"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task DeleteSelectedAsync()
	{
		if (!CanDelete || Selected is null)
		{
			throw new InvalidOperationException();
		}

		await DeleteAsync(Selected);
	}

	/// <summary>Deletes the specified row.</summary>
	/// <param name="row">The row to delete.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	protected abstract Task DeleteAsync(TRow row);
}