// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls;

using Tracking.Finance.Interfaces.Desktop.ViewModels.Events;
using Tracking.Finance.Interfaces.Desktop.Views;
using Tracking.Finance.Interfaces.WebApi.Client;
using Tracking.Finance.Interfaces.WebApi.V1_0.Products;

namespace Tracking.Finance.Interfaces.Desktop.ViewModels
{
	public sealed class ProductCreationViewModel : ViewModelBase<ProductCreationView>
	{
		private readonly IFinanceClient _financeClient;
		private string? _name;
		private UnitModel? _selectedUnit;
		private string? _description;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProductCreationViewModel"/> class.
		/// </summary>
		public ProductCreationViewModel()
			: this(new FinanceClient())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProductCreationViewModel"/> class.
		/// </summary>
		/// <param name="financeClient">Finance API client for getting/saving data.</param>
		public ProductCreationViewModel(IFinanceClient financeClient)
		{
			_financeClient = financeClient;

			Units = GetUnitsAsync();
			UnitSelector = (_, item) => ((UnitModel)item).Name;
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
		public UnitModel? SelectedUnit
		{
			get => _selectedUnit;
			set => SetAndNotify(ref _selectedUnit, value, nameof(SelectedUnit));
		}

		/// <summary>
		/// Gets a collection of all available units.
		/// </summary>
		public Task<List<UnitModel>> Units { get; }

		public AutoCompleteSelector<object> UnitSelector { get; }

		/// <summary>
		/// Gets a value indicating whether a product can be created from currently provided values.
		/// </summary>
		public bool CanCreate => !string.IsNullOrWhiteSpace(Name);

		/// <summary>
		/// Creates a new product from the provided values.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task CreateAsync()
		{
			var creationModel = new ProductCreationModel
			{
				Name = Name,
				Description = Description,
				UnitId = SelectedUnit?.Id,
			};

			var productId = await _financeClient.CreateProductAsync(creationModel).ConfigureAwait(false);
			OnProductCreated(productId);
		}

		private async Task<List<UnitModel>> GetUnitsAsync()
		{
			return await _financeClient.GetUnitsAsync().ConfigureAwait(false);
		}

		private void OnProductCreated(Guid productId)
		{
			ProductCreated?.Invoke(this, new(productId));
		}
	}
}
