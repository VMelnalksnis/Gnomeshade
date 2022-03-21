// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;

namespace Gnomeshade.Interfaces.Avalonia.Core.Counterparties;

/// <summary>Updates the values of a single counterparty.</summary>
public sealed class CounterpartyUpdateViewModel : UpsertionViewModel
{
	private readonly Counterparty? _originalCounterparty;

	private string? _name;

	private CounterpartyUpdateViewModel(IGnomeshadeClient gnomeshadeClient)
		: base(gnomeshadeClient)
	{
	}

	private CounterpartyUpdateViewModel(IGnomeshadeClient gnomeshadeClient, Counterparty originalCounterparty)
		: this(gnomeshadeClient)
	{
		_originalCounterparty = originalCounterparty;

		_name = _originalCounterparty.Name;
	}

	/// <summary>Gets or sets the name of the counterparty.</summary>
	public string? Name
	{
		get => _name;
		set => SetAndNotifyWithGuard(ref _name, value, nameof(Name), nameof(CanSave));
	}

	/// <inheritdoc />
	public override bool CanSave => !string.IsNullOrWhiteSpace(Name);

	/// <summary>Asynchronously creates a new instance of the <see cref="CounterpartyUpdateViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="id">The id of the counterparty to edit.</param>
	/// <returns>A new instance of <see cref="CounterpartyUpdateViewModel"/>.</returns>
	public static async Task<CounterpartyUpdateViewModel> CreateAsync(
		IGnomeshadeClient gnomeshadeClient,
		Guid? id = null)
	{
		if (id is null)
		{
			return new(gnomeshadeClient);
		}

		var counterparty = await gnomeshadeClient.GetCounterpartyAsync(id.Value).ConfigureAwait(false);
		return new(gnomeshadeClient, counterparty);
	}

	/// <inheritdoc />
	public override async Task SaveAsync()
	{
		var counterparty = new CounterpartyCreationModel
		{
			Name = Name,
		};

		var id = _originalCounterparty?.Id ?? Guid.NewGuid();
		await GnomeshadeClient.PutCounterpartyAsync(id, counterparty).ConfigureAwait(false);
		OnUpserted(id);
	}
}
