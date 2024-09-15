// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Projects;

/// <summary>Overview of all projects.</summary>
public sealed class ProjectViewModel : OverviewViewModel<ProjectRow, ProjectUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private ProjectUpsertionViewModel _details;

	/// <summary>Initializes a new instance of the <see cref="ProjectViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	public ProjectViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IDateTimeZoneProvider dateTimeZoneProvider)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_details = new(activityService, gnomeshadeClient, dateTimeZoneProvider, null);

		_details.Upserted += DetailsOnUpserted;
	}

	/// <inheritdoc />
	public override ProjectUpsertionViewModel Details
	{
		get => _details;
		set
		{
			_details.Upserted -= DetailsOnUpserted;
			SetAndNotify(ref _details, value);
			_details.Upserted += DetailsOnUpserted;
		}
	}

	/// <inheritdoc />
	public override async Task UpdateSelection()
	{
		Details = new(ActivityService, _gnomeshadeClient, _dateTimeZoneProvider, Selected?.Id);
		await Details.RefreshAsync();
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var projects = await _gnomeshadeClient.GetProjectsAsync();

		var overviews = projects
			.Select(project => new ProjectRow(project))
			.ToList();

		var selected = Selected;
		var sort = DataGridView.SortDescriptions;

		Rows = new(overviews);
		DataGridView.SortDescriptions.AddRange(sort);
		Selected = Rows.SingleOrDefault(overview => overview.Id == selected?.Id);

		if (Selected is null)
		{
			await Details.RefreshAsync();
		}

		foreach (var overview in overviews)
		{
			var purchases = await _gnomeshadeClient.GetProjectPurchasesAsync(overview.Id);
			var purchasesByCurrency = purchases.GroupBy(purchase => purchase.CurrencyId).ToArray();
			if (purchasesByCurrency is [])
			{
				continue;
			}

			if (purchasesByCurrency is not [var group])
			{
				throw new NotImplementedException("Multiple currencies are not supported");
			}

			overview.Total = group.Sum(purchase => purchase.Price);
		}
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(ProjectRow row)
	{
		await _gnomeshadeClient.DeleteProjectAsync(row.Id);
		Details = new(ActivityService, _gnomeshadeClient, _dateTimeZoneProvider, null);
	}

	private async void DetailsOnUpserted(object? sender, UpsertedEventArgs e)
	{
		await RefreshAsync();
	}
}
