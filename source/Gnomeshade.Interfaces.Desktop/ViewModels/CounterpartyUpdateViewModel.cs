// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.Desktop.ViewModels.Events;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;

namespace Gnomeshade.Interfaces.Desktop.ViewModels;

/// <summary>Updates the values of a single counterparty.</summary>
public sealed class CounterpartyUpdateViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Counterparty _originalCounterparty;
	private string _name;

	private CounterpartyUpdateViewModel(IGnomeshadeClient gnomeshadeClient, Counterparty originalCounterparty)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_originalCounterparty = originalCounterparty;

		_name = _originalCounterparty.Name;
	}

	/// <summary>Raised when a counterparty has been updated.</summary>
	public event EventHandler<CounterpartyUpdatedEventArgs>? Updated;

	/// <summary>Gets or sets the name of the counterparty.</summary>
	public string Name
	{
		get => _name;
		set => SetAndNotifyWithGuard(ref _name, value, nameof(Name), nameof(CanUpdate));
	}

	/// <summary>Gets a value indicating whether <see cref="UpdateAsync"/> can be called.</summary>
	public bool CanUpdate => !string.IsNullOrWhiteSpace(Name);

	/// <summary>Asynchronously creates a new instance of the <see cref="CounterpartyUpdateViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="id">The id of the counterparty to edit.</param>
	/// <returns>A new instance of <see cref="CounterpartyUpdateViewModel"/>.</returns>
	public static async Task<CounterpartyUpdateViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient, Guid id)
	{
		var counterparty = await gnomeshadeClient.GetCounterpartyAsync(id);
		return new(gnomeshadeClient, counterparty);
	}

	/// <summary>Updates the counterparty with the set values.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task UpdateAsync()
	{
		var counterparty = new CounterpartyCreationModel
		{
			Name = Name,
		};

		await _gnomeshadeClient.PutCounterpartyAsync(_originalCounterparty.Id, counterparty);
		Updated?.Invoke(this, new(_originalCounterparty.Id));
	}
}
