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
using Tracking.Finance.Interfaces.WebApi.V1_0.Accounts;

namespace Tracking.Finance.Interfaces.Desktop.ViewModels
{
	public sealed class AccountCreationViewModel : ViewModelBase<AccountCreationView>
	{
		private readonly IFinanceClient _financeClient;

		private string? _name;
		private CurrencyModel? _preferredCurrency;

		/// <summary>
		/// Initializes a new instance of the <see cref="AccountCreationViewModel"/> class.
		/// </summary>
		public AccountCreationViewModel()
			: this(new FinanceClient())
		{
		}

		public AccountCreationViewModel(IFinanceClient financeClient)
		{
			_financeClient = financeClient;
			Currencies = GetCurrenciesAsync();
		}

		/// <summary>
		/// Raised when a new account has been successfully created.
		/// </summary>
		public event EventHandler<AccountCreatedEventArgs>? AccountCreated;

		public AutoCompleteSelector<object> CurrencySelector { get; } = (_, item) => ((CurrencyModel)item).Name;

		/// <summary>
		/// Gets a collection of available currencies.
		/// </summary>
		public Task<List<CurrencyModel>> Currencies { get; }

		/// <summary>
		/// Gets or sets the name of the account.
		/// </summary>
		public string? Name
		{
			get => _name;
			set => SetAndNotifyWithGuard(ref _name, value, nameof(Name), nameof(CanCreate));
		}

		/// <summary>
		/// Gets or sets the preferred current of the account.
		/// </summary>
		public CurrencyModel? PreferredCurrency
		{
			get => _preferredCurrency;
			set => SetAndNotifyWithGuard(ref _preferredCurrency, value, nameof(PreferredCurrency), nameof(CanCreate));
		}

		/// <summary>
		/// Gets a value indicating whether or not the account can be created.
		/// </summary>
		public bool CanCreate =>
			!string.IsNullOrWhiteSpace(Name) &&
			PreferredCurrency is not null;

		/// <summary>
		/// Creates a new account with the given values.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task CreateAsync()
		{
			var accountCreationModel = new AccountCreationModel
			{
				Name = Name,
				PreferredCurrencyId = PreferredCurrency?.Id,
				Currencies = new() { new() { CurrencyId = PreferredCurrency?.Id } },
			};

			var id = await _financeClient.CreateAccountAsync(accountCreationModel).ConfigureAwait(true);
			OnAccountCreated(id);
		}

		private async Task<List<CurrencyModel>> GetCurrenciesAsync()
		{
			return await _financeClient.GetCurrenciesAsync().ConfigureAwait(false);
		}

		private void OnAccountCreated(Guid id)
		{
			AccountCreated?.Invoke(this, new(id));
		}
	}
}
