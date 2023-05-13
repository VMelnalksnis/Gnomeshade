// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Owners;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Accesses;

/// <summary>Form for creating or updating an ownership.</summary>
public sealed partial class OwnerUpsertionViewModel : UpsertionViewModel
{
	private List<OwnershipRow> _originalOwnerships;
	private List<User> _users;

	/// <summary>Gets a collection of all ownerships associated with the selected owner.</summary>
	[Notify(Setter.Private)]
	private ObservableCollection<OwnershipRow> _ownerships;

	/// <summary>Gets a collection of all counterparties.</summary>
	[Notify(Setter.Private)]
	private List<Counterparty> _counterparties;

	/// <summary>Gets a collection of all available access levels.</summary>
	[Notify(Setter.Private)]
	private List<Access> _accesses;

	/// <summary>Gets or sets the name of the owner.</summary>
	[Notify]
	private string? _name;

	/// <summary>Gets or sets the access level of the ownership.</summary>
	[Notify]
	private Access? _selectedAccess;

	/// <summary>Gets or sets the selected user from <see cref="Counterparties"/>.</summary>
	[Notify]
	private Counterparty? _selectedCounterparty;

	/// <summary>Gets or sets the selected ownership from <see cref="Ownerships"/>.</summary>
	[Notify]
	private OwnershipRow? _selectedRow;

	/// <summary>Initializes a new instance of the <see cref="OwnerUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="id">The id of the ownership to edit.</param>
	public OwnerUpsertionViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient, Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_originalOwnerships = new();
		_ownerships = new();
		_accesses = new();
		_users = new();
		_counterparties = new();

		Id = id;
	}

	/// <summary>Gets a delegate for formatting an access level in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> AccessSelector => AutoCompleteSelectors.Access;

	/// <summary>Gets a delegate for formatting a counterparty in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CounterpartySelector => AutoCompleteSelectors.Counterparty;

	/// <inheritdoc />
	public override bool CanSave => Name is not null;

	/// <summary>Gets a value indicating whether an <see cref="OwnershipRow"/> can be saved.</summary>
	public bool CanSaveRow =>
		SelectedAccess is not null &&
		SelectedCounterparty is not null &&
		!Ownerships.Any(row => row.AccessId == SelectedAccess.Id && row.UserId == SelectedCounterparty.Id);

	/// <summary>Gets a collection of users that can be given access to this owner.</summary>
	public List<Counterparty> AddableCounterparties => Counterparties
		.Where(counterparty => Ownerships.All(ownership => ownership.UserId != counterparty.Id))
		.ToList();

	/// <summary>Called when <see cref="SelectedRow"/> has been updated.</summary>
	public void UpdateSelection()
	{
		if (SelectedRow is not { } row)
		{
			return;
		}

		SelectedAccess = Accesses.Single(access => access.Id == row.AccessId);
		SelectedCounterparty = Counterparties.SingleOrDefault(counterparty => counterparty.Id == row.UserId);
	}

	/// <summary>
	/// Adds a new row to <see cref="Ownerships"/> if <see cref="SelectedRow"/> is <c>null</c>,
	/// otherwise updates the <see cref="SelectedRow"/>.
	/// </summary>
	public void SaveRow()
	{
		ArgumentNullException.ThrowIfNull(SelectedAccess);
		ArgumentNullException.ThrowIfNull(SelectedCounterparty);

		if (SelectedRow is { } row)
		{
			row.SetAccess(SelectedAccess);
			row.SetUser(SelectedCounterparty);
			SelectedRow = null;
		}
		else
		{
			Ownerships.Add(new(SelectedAccess, SelectedCounterparty));
		}
	}

	/// <summary>Removes <see cref="SelectedRow"/> from <see cref="Ownerships"/>.</summary>
	public void RemoveRow()
	{
		ArgumentNullException.ThrowIfNull(SelectedRow);

		Ownerships.Remove(SelectedRow);
		SelectedRow = null;
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		_users = await GnomeshadeClient.GetUsersAsync();
		Accesses = await GnomeshadeClient.GetAccessesAsync();
		var userCounterparty = await GnomeshadeClient.GetMyCounterpartyAsync();
		var counterparties = await GnomeshadeClient.GetCounterpartiesAsync();
		Counterparties = counterparties.Where(counterparty => counterparty.Id != userCounterparty.Id).ToList();
		if (Id is null)
		{
			return;
		}

		var owner = (await GnomeshadeClient.GetOwnersAsync()).Single(owner => owner.Id == Id);
		Name = owner.Name;

		var ownerships = await GnomeshadeClient.GetOwnershipsAsync();
		var rows = ownerships
			.Where(ownership => ownership.OwnerId == owner.Id)
			.GetRows(Accesses, _users, Counterparties)
			.ToList();
		Ownerships = new(rows);
		_originalOwnerships = Ownerships.ToList();
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var ownerId = Id ?? Guid.NewGuid();
		await GnomeshadeClient.PutOwnerAsync(ownerId, new() { Name = Name! });

		var userCounterparty = await GnomeshadeClient.GetMyCounterpartyAsync();
		var ownerAccess = Accesses.Single(access => access.Name is "Owner");
		await GnomeshadeClient.PutOwnershipAsync(ownerId, new()
		{
			AccessId = ownerAccess.Id,
			OwnerId = ownerId,
			UserId = userCounterparty.Id,
		});

		foreach (var ownershipRow in Ownerships)
		{
			var ownershipId = ownershipRow.Id ??= Guid.NewGuid();

			await GnomeshadeClient.PutOwnershipAsync(ownershipId, new()
			{
				AccessId = ownershipRow.AccessId,
				OwnerId = ownerId,
				UserId = ownershipRow.UserId,
			});
		}

		foreach (var ownership in _originalOwnerships.Where(ownership => !Ownerships.Contains(ownership)))
		{
			if (ownership.Id is { } ownershipId)
			{
				await GnomeshadeClient.DeleteOwnershipAsync(ownershipId);
			}
		}

		return ownerId;
	}
}
