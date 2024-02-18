// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.WebApi.Client;

namespace Gnomeshade.Avalonia.Core.Counterparties;

/// <summary>List of all counterparties.</summary>
public sealed class CounterpartyViewModel : OverviewViewModel<CounterpartyRow, CounterpartyUpsertionViewModel>
{
	private static readonly DataGridSortDescription[] _sortDescriptions =
	[
		new DataGridComparerSortDescription(new CounterpartyComparer(), ListSortDirection.Ascending),
	];

	private readonly IGnomeshadeClient _gnomeshadeClient;

	private CounterpartyUpsertionViewModel _details;

	/// <summary>Initializes a new instance of the <see cref="CounterpartyViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	public CounterpartyViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_details = new(activityService, gnomeshadeClient, null);

		DataGridView.SortDescriptions.AddRange(_sortDescriptions);

		Filter = new(activityService);

		Filter.PropertyChanged += FilterOnPropertyChanged;
		_details.Upserted += DetailsOnUpserted;
	}

	/// <inheritdoc />
	public override CounterpartyUpsertionViewModel Details
	{
		get => _details;
		set
		{
			Details.Upserted -= DetailsOnUpserted;
			SetAndNotify(ref _details, value);
			Details.Upserted += DetailsOnUpserted;
		}
	}

	/// <summary>Gets the counterparty filter.</summary>
	public CounterpartyFilter Filter { get; }

	/// <inheritdoc />
	public override Task UpdateSelection()
	{
		Details = new(ActivityService, _gnomeshadeClient, Selected?.Id);
		return Details.RefreshAsync();
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var counterparties = await _gnomeshadeClient.GetCounterpartiesAsync();

		var selected = Selected;
		var sort = DataGridView.SortDescriptions;

		var counterpartyRows = counterparties.Select(counterparty => new CounterpartyRow(counterparty)).ToList();
		Rows = new(counterpartyRows);

		DataGridView.SortDescriptions.AddRange(sort);
		DataGridView.Filter = Filter.Filter;

		Selected = Rows.SingleOrDefault(row => row.Id == selected?.Id);
	}

	/// <inheritdoc />
	protected override Task DeleteAsync(CounterpartyRow row)
	{
		const string message = "Deleting counterparties is not yet supported";
		throw new NotSupportedException(message);
	}

	private async void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync();
	}

	private void FilterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		DataGridView.Refresh();
	}
}
