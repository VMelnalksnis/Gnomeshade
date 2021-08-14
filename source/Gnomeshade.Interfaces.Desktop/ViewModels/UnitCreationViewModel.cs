// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Interfaces.Desktop.ViewModels.Events;
using Gnomeshade.Interfaces.Desktop.Views;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Interfaces.Desktop.ViewModels
{
	public sealed class UnitCreationViewModel : ViewModelBase<UnitCreationView>
	{
		private readonly IGnomeshadeClient _gnomeshadeClient;
		private string? _name;
		private Unit? _parentUnit;
		private decimal? _multiplier;

		/// <summary>
		/// Initializes a new instance of the <see cref="UnitCreationViewModel"/> class.
		/// </summary>
		public UnitCreationViewModel()
			: this(new GnomeshadeClient())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnitCreationViewModel"/> class.
		/// </summary>
		/// <param name="gnomeshadeClient">Finance API client for getting/saving data.</param>
		public UnitCreationViewModel(IGnomeshadeClient gnomeshadeClient)
		{
			_gnomeshadeClient = gnomeshadeClient;

			Units = GetUnitsAsync();
			UnitSelector = (_, item) => ((Unit)item).Name;
		}

		/// <summary>
		/// Raised when a new unit has been successfully created.
		/// </summary>
		public event EventHandler<UnitCreatedEventArgs>? UnitCreated;

		/// <summary>
		/// Gets or sets the name of the unit.
		/// </summary>
		public string? Name
		{
			get => _name;
			set => SetAndNotifyWithGuard(ref _name, value, nameof(Name), nameof(CanCreate));
		}

		/// <summary>
		/// Gets or sets the unit on which this unit is based on.
		/// </summary>
		public Unit? ParentUnit
		{
			get => _parentUnit;
			set => SetAndNotifyWithGuard(ref _parentUnit, value, nameof(ParentUnit), nameof(CanCreate));
		}

		/// <summary>
		/// Gets a collection of all available units.
		/// </summary>
		public Task<List<Unit>> Units { get; }

		public AutoCompleteSelector<object> UnitSelector { get; }

		/// <summary>
		/// Gets or sets a multiplier for converting from this unit to <see cref="ParentUnit"/>.
		/// </summary>
		public decimal? Multiplier
		{
			get => _multiplier;
			set => SetAndNotifyWithGuard(ref _multiplier, value, nameof(Multiplier), nameof(ParentUnit));
		}

		/// <summary>
		/// Gets a value indicating whether or not a unit can be created from the currently specified values.
		/// </summary>
		public bool CanCreate =>
			!string.IsNullOrWhiteSpace(Name) &&
			((ParentUnit is null && Multiplier is null) || (ParentUnit is not null && Multiplier is not null));

		/// <summary>
		/// Creates a new unit form the specified values.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task CreateAsync()
		{
			var unit = new UnitCreationModel
			{
				Name = Name,
				ParentUnitId = ParentUnit?.Id,
				Multiplier = Multiplier,
			};

			var unitId = await _gnomeshadeClient.CreateUnitAsync(unit).ConfigureAwait(false);
			OnUnitCreated(unitId);
		}

		private async Task<List<Unit>> GetUnitsAsync()
		{
			return await _gnomeshadeClient.GetUnitsAsync().ConfigureAwait(false);
		}

		private void OnUnitCreated(Guid unitId)
		{
			UnitCreated?.Invoke(this, new(unitId));
		}
	}
}
