// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Products;

/// <summary>Overview and editing of all units.</summary>
public sealed partial class UnitViewModel : OverviewViewModel<UnitRow, UnitUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	/// <summary>Gets or sets the selected unit from <see cref="OverviewViewModel{TRow,TUpsertion}.DataGridView"/>.</summary>
	[Notify]
	private UnitRow? _selectedUnit;

	private UnitUpsertionViewModel _details;

	/// <summary>Initializes a new instance of the <see cref="UnitViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	public UnitViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_details = new(activityService, gnomeshadeClient, null);

		PropertyChanged += OnPropertyChanged;
		Details.Upserted += DetailedOnUpserted;
	}

	/// <inheritdoc />
	public override UnitUpsertionViewModel Details
	{
		get => _details;
		set
		{
			Details.Upserted -= DetailedOnUpserted;
			SetAndNotify(ref _details, value);
			Details.Upserted += DetailedOnUpserted;
		}
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var unitRows = (await _gnomeshadeClient.GetUnitRowsAsync()).ToArray();
		var sortDescriptions = DataGridView.SortDescriptions;
		var selected = Selected;

		Rows = new(unitRows);
		DataGridView.SortDescriptions.AddRange(sortDescriptions);
		Selected = Rows.SingleOrDefault(overview => overview.Id == selected?.Id);
		if (Selected is null)
		{
			await Details.RefreshAsync();
		}
	}

	/// <inheritdoc />
	protected override Task DeleteAsync(UnitRow row) => throw new System.NotImplementedException();

	private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not nameof(SelectedUnit))
		{
			return;
		}

		Details = new(ActivityService, _gnomeshadeClient, SelectedUnit?.Id);
		await Details.RefreshAsync();
	}

	private async void DetailedOnUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync();
	}
}
