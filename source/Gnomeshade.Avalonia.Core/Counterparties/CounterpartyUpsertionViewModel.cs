// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Counterparties;

/// <summary>Updates the values of a single counterparty.</summary>
public sealed partial class CounterpartyUpsertionViewModel : UpsertionViewModel
{
	/// <summary>Gets or sets the name of the counterparty.</summary>
	[Notify]
	private string? _name;

	/// <summary>Initializes a new instance of the <see cref="CounterpartyUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="id">The id of the counterparty to edit.</param>
	public CounterpartyUpsertionViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		Id = id;
	}

	/// <inheritdoc />
	public override bool CanSave => !string.IsNullOrWhiteSpace(Name);

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		if (Id is not { } id)
		{
			return;
		}

		var counterparty = await GnomeshadeClient.GetCounterpartyAsync(id);
		Name = counterparty.Name;
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var counterparty = new CounterpartyCreation { Name = Name };
		var id = Id ?? Guid.NewGuid();
		await GnomeshadeClient.PutCounterpartyAsync(id, counterparty);
		return id;
	}
}
