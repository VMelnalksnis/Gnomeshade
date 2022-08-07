// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Avalonia.Core.Counterparties;

/// <summary>Merges one counterparty and its accounts into another.</summary>
public sealed class CounterpartyMergeViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private DataGridItemCollectionView<CounterpartyRow> _sourceCounterparties;
	private DataGridItemCollectionView<CounterpartyRow> _targetCounterparties;
	private CounterpartyRow? _sourceCounterparty;
	private CounterpartyRow? _targetCounterparty;

	/// <summary>Initializes a new instance of the <see cref="CounterpartyMergeViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	public CounterpartyMergeViewModel(IGnomeshadeClient gnomeshadeClient)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_sourceCounterparties = new(Array.Empty<CounterpartyRow>());
		_targetCounterparties = new(Array.Empty<CounterpartyRow>());
	}

	/// <summary>Gets a grid view of all counterparties that can be merged.</summary>
	public DataGridCollectionView SourceDataGridView => SourceCounterparties;

	/// <summary>Gets a typed collection of counterparties in <see cref="SourceDataGridView"/>.</summary>
	public DataGridItemCollectionView<CounterpartyRow> SourceCounterparties
	{
		get => _sourceCounterparties;
		private set => SetAndNotifyWithGuard(ref _sourceCounterparties, value, nameof(SourceCounterparties), nameof(SourceDataGridView));
	}

	/// <summary>Gets or sets the counterparty to merge.</summary>
	public CounterpartyRow? SourceCounterparty
	{
		get => _sourceCounterparty;
		set => SetAndNotifyWithGuard(ref _sourceCounterparty, value, nameof(SourceCounterparty), nameof(CanMerge));
	}

	/// <summary>Gets a grid view of all counterparties that can be merged into.</summary>
	public DataGridCollectionView TargetDataGridView => TargetCounterparties;

	/// <summary>Gets a typed collection of counterparties in <see cref="TargetDataGridView"/>.</summary>
	public DataGridItemCollectionView<CounterpartyRow> TargetCounterparties
	{
		get => _targetCounterparties;
		private set => SetAndNotifyWithGuard(ref _targetCounterparties, value, nameof(TargetCounterparties), nameof(TargetDataGridView));
	}

	/// <summary>Gets or sets the counterparty to merge into.</summary>
	public CounterpartyRow? TargetCounterparty
	{
		get => _targetCounterparty;
		set => SetAndNotifyWithGuard(ref _targetCounterparty, value, nameof(TargetCounterparty), nameof(CanMerge));
	}

	/// <summary>Gets a value indicating whether the counterparties can be merged.</summary>
	public bool CanMerge =>
		SourceCounterparty is not null &&
		TargetCounterparty is not null &&
		SourceCounterparty.Id != TargetCounterparty.Id;

	/// <summary>Merges <see cref="SourceCounterparty"/> into <see cref="TargetCounterparty"/>.</summary>
	/// <exception cref="InvalidOperationException">One of the counterparties is not selected.</exception>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task MergeAsync()
	{
		if (TargetCounterparty is null || SourceCounterparty is null)
		{
			throw new InvalidOperationException();
		}

		await _gnomeshadeClient.MergeCounterpartiesAsync(TargetCounterparty.Id, SourceCounterparty.Id).ConfigureAwait(false);
		await RefreshAsync();
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var counterparties = await _gnomeshadeClient.GetCounterpartiesAsync();
		var counterpartyRows = counterparties.Select(counterparty => new CounterpartyRow(counterparty, 0)).ToList();

		// Preserve the current sorting and selected target, but clear the selected source.
		var sourceSort = SourceDataGridView.SortDescriptions;
		var targetSort = TargetDataGridView.SortDescriptions;
		var targetCounterparty = TargetCounterparty;

		SourceCounterparties = new(counterpartyRows);
		SourceDataGridView.SortDescriptions.AddRange(sourceSort);
		SourceCounterparty = null;

		TargetCounterparties = new(counterpartyRows);
		TargetDataGridView.SortDescriptions.AddRange(targetSort);
		TargetCounterparty = TargetCounterparties.SingleOrDefault(counterparty => counterparty.Id == targetCounterparty?.Id);
	}
}
