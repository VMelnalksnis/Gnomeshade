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
	/// <summary>
	/// Form for creating a single new product.
	/// </summary>
	public sealed class ProductCreationViewModel : ViewModelBase<ProductCreationView>
	{
		private readonly IGnomeshadeClient _gnomeshadeClient;
		private string? _name;
		private Unit? _selectedUnit;
		private string? _description;

		private ProductCreationViewModel(IGnomeshadeClient gnomeshadeClient, List<Unit> units)
		{
			_gnomeshadeClient = gnomeshadeClient;
			Units = units;

			UnitSelector = (_, item) => ((Unit)item).Name;
		}

		/// <summary>
		/// Raised when a new product has been successfully created.
		/// </summary>
		public event EventHandler<ProductCreatedEventArgs>? ProductCreated;

		/// <summary>
		/// Gets or sets the name of the product.
		/// </summary>
		public string? Name
		{
			get => _name;
			set => SetAndNotifyWithGuard(ref _name, value, nameof(Name), nameof(CanCreate));
		}

		/// <summary>
		/// Gets or sets the description of the product.
		/// </summary>
		public string? Description
		{
			get => _description;
			set => SetAndNotify(ref _description, value, nameof(Description));
		}

		/// <summary>
		/// Gets or sets the unit in which an amount of this product is measured in.
		/// </summary>
		public Unit? SelectedUnit
		{
			get => _selectedUnit;
			set => SetAndNotify(ref _selectedUnit, value, nameof(SelectedUnit));
		}

		/// <summary>
		/// Gets a collection of all available units.
		/// </summary>
		public List<Unit> Units { get; }

		public AutoCompleteSelector<object> UnitSelector { get; }

		/// <summary>
		/// Gets a value indicating whether a product can be created from currently provided values.
		/// </summary>
		public bool CanCreate => !string.IsNullOrWhiteSpace(Name);

		/// <summary>
		/// Initializes a new instance of the <see cref="ProductCreationViewModel"/> class.
		/// </summary>
		/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
		/// <returns>A new instance of the <see cref="ProductCreationViewModel"/> class.</returns>
		public static async Task<ProductCreationViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
		{
			var units = await gnomeshadeClient.GetUnitsAsync();
			return new(gnomeshadeClient, units);
		}

		/// <summary>
		/// Creates a new product from the provided values.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task CreateProductAsync()
		{
			var creationModel = new ProductCreationModel
			{
				Name = Name,
				Description = Description,
				UnitId = SelectedUnit?.Id,
			};

			var productId = await _gnomeshadeClient.PutProductAsync(creationModel).ConfigureAwait(false);
			OnProductCreated(productId);
		}

		private void OnProductCreated(Guid productId)
		{
			ProductCreated?.Invoke(this, new(productId));
		}
	}
}
