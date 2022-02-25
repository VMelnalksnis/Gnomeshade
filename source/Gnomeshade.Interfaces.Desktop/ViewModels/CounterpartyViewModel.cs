// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.Interfaces.Avalonia.Core;
using Gnomeshade.Interfaces.Desktop.Models;
using Gnomeshade.Interfaces.Desktop.ViewModels.Binding;
using Gnomeshade.Interfaces.Desktop.ViewModels.Events;
using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Desktop.ViewModels;

/// <summary>List of all counterparties.</summary>
public sealed class CounterpartyViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private CounterpartyRow? _selectedCounterparty;
	private CounterpartyUpdateViewModel? _counterparty;
	private DataGridItemCollectionView<CounterpartyRow> _counterparties;

	private CounterpartyViewModel(IGnomeshadeClient gnomeshadeClient, List<CounterpartyRow> counterpartyRows)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_counterparties = new(counterpartyRows);

		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Gets a grid view of all counterparties.</summary>
	public DataGridCollectionView DataGridView => Counterparties;

	/// <summary>Gets a typed collection of accounts in <see cref="DataGridView"/>.</summary>
	public DataGridItemCollectionView<CounterpartyRow> Counterparties
	{
		get => _counterparties;
		private set => SetAndNotifyWithGuard(ref _counterparties, value, nameof(Counterparties), nameof(DataGridView));
	}

	/// <summary>Gets or sets the selected counterparty.</summary>
	public CounterpartyRow? SelectedCounterparty
	{
		get => _selectedCounterparty;
		set => SetAndNotifyWithGuard(ref _selectedCounterparty, value, nameof(SelectedCounterparty), nameof(Counterparty));
	}

	/// <summary>Gets the counterparty update view model.</summary>
	public CounterpartyUpdateViewModel? Counterparty
	{
		get => _counterparty;
		private set
		{
			if (Counterparty is not null)
			{
				Counterparty.Updated -= CounterpartyOnUpdated;
			}

			SetAndNotify(ref _counterparty, value);
			if (Counterparty is not null)
			{
				Counterparty.Updated += CounterpartyOnUpdated;
			}
		}
	}

	/// <summary>Asynchronously creates a new instance of the <see cref="CounterpartyViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <returns>A new instance of <see cref="CounterpartyViewModel"/>.</returns>
	public static async Task<CounterpartyViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
	{
		var counterparties = await gnomeshadeClient.GetCounterpartiesAsync();
		var counterpartyRows = counterparties.Select(counterparty => new CounterpartyRow(counterparty)).ToList();
		return new(gnomeshadeClient, counterpartyRows);
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(SelectedCounterparty) && SelectedCounterparty is not null)
		{
			Counterparty = Task.Run(() =>
				CounterpartyUpdateViewModel.CreateAsync(_gnomeshadeClient, SelectedCounterparty.Id)).Result;
		}
	}

	private void CounterpartyOnUpdated(object? sender, CounterpartyUpdatedEventArgs e)
	{
		var counterparties = Task.Run(() => _gnomeshadeClient.GetCounterpartiesAsync()).Result;
		var counterpartyRows = counterparties.Select(counterparty => new CounterpartyRow(counterparty));
		Counterparties = new(counterpartyRows);
	}
}
