// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

namespace Gnomeshade.Avalonia.Core.Accesses;

/// <summary>Overview of all owners.</summary>
public sealed class OwnerViewModel : OverviewViewModel<OwnerRow, OwnerUpsertionViewModel>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private OwnerUpsertionViewModel _details;

	/// <summary>Initializes a new instance of the <see cref="OwnerViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	public OwnerViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_details = new(activityService, gnomeshadeClient, null);
	}

	/// <inheritdoc />
	public override OwnerUpsertionViewModel Details
	{
		get => _details;
		set => SetAndNotify(ref _details, value);
	}

	/// <inheritdoc />
	public override Task UpdateSelection()
	{
		Details = new(ActivityService, _gnomeshadeClient, Selected?.Id);
		return Details.RefreshAsync();
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var owners = await _gnomeshadeClient.GetOwnersAsync();
		var ownerships = await _gnomeshadeClient.GetOwnershipsAsync();
		var accesses = await _gnomeshadeClient.GetAccessesAsync();
		var users = await _gnomeshadeClient.GetUsersAsync();
		var counterparties = await _gnomeshadeClient.GetCounterpartiesAsync();

		var rows = owners
			.Select(owner =>
			{
				var ownershipRows = ownerships
					.Where(ownership => ownership.OwnerId == owner.Id)
					.GetRows(accesses, users, counterparties)
					.ToList();

				return new OwnerRow(owner.Id, owner.Name, ownershipRows);
			})
			.ToList();

		var selected = Selected;
		Rows = new(rows);
		Selected = Rows.SingleOrDefault(row => row.Id == selected?.Id);
		await Details.RefreshAsync();
	}

	/// <inheritdoc />
	protected override async Task DeleteAsync(OwnerRow row)
	{
		await _gnomeshadeClient.DeleteOwnerAsync(row.Id);
	}
}
