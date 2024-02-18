// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.WebApi.Client;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Counterparties;

/// <summary>Merges one counterparty and its accounts into another.</summary>
public sealed partial class CounterpartyMergeViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	/// <summary>Gets a typed collection of counterparties in <see cref="SourceDataGridView"/>.</summary>
	[Notify(Setter.Private)]
	private DataGridItemCollectionView<CounterpartyRow> _sourceCounterparties;

	/// <summary>Gets a typed collection of counterparties in <see cref="TargetDataGridView"/>.</summary>
	[Notify(Setter.Private)]
	private DataGridItemCollectionView<CounterpartyRow> _targetCounterparties;

	/// <summary>Gets or sets the counterparty to merge.</summary>
	[Notify]
	private CounterpartyRow? _sourceCounterparty;

	/// <summary>Gets or sets the counterparty to merge into.</summary>
	[Notify]
	private CounterpartyRow? _targetCounterparty;

	/// <summary>Initializes a new instance of the <see cref="CounterpartyMergeViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	public CounterpartyMergeViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_sourceCounterparties = new(Array.Empty<CounterpartyRow>());
		_targetCounterparties = new(Array.Empty<CounterpartyRow>());
	}

	/// <summary>Gets a grid view of all counterparties that can be merged.</summary>
	public DataGridCollectionView SourceDataGridView => SourceCounterparties;

	/// <summary>Gets a grid view of all counterparties that can be merged into.</summary>
	public DataGridCollectionView TargetDataGridView => TargetCounterparties;

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

		using var activity = BeginActivity("Merging counterparties");
		await _gnomeshadeClient.MergeCounterpartiesAsync(TargetCounterparty.Id, SourceCounterparty.Id);
		await RefreshAsync();
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var counterparties = await _gnomeshadeClient.GetCounterpartiesAsync();
		var counterpartyRows = counterparties.Select(counterparty => new CounterpartyRow(counterparty)).ToList();

		// Preserve the current sorting and selected target, but clear the selected source.
		var sourceSort = SourceDataGridView.SortDescriptions;
		var targetSort = TargetDataGridView.SortDescriptions;
		var targetCounterparty = TargetCounterparty;

		SourceCounterparties = new(counterpartyRows);
		SourceDataGridView.SortDescriptions.AddRange(sourceSort);
		SourceCounterparty = null;

		TargetCounterparties = new(counterpartyRows);
		TargetDataGridView.SortDescriptions.AddRange(targetSort);
		TargetCounterparty = TargetCounterparties
			.SingleOrDefault(counterparty => counterparty.Id == targetCounterparty?.Id);
	}
}
