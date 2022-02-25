// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;

namespace Gnomeshade.Interfaces.Avalonia.Core.Accounts;

/// <summary>
/// Form for creating a single new account.
/// </summary>
public sealed class AccountCreationViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private string? _name;
	private Currency? _preferredCurrency;

	private AccountCreationViewModel(IGnomeshadeClient gnomeshadeClient, List<Currency> currencies)
	{
		_gnomeshadeClient = gnomeshadeClient;
		Currencies = currencies;
	}

	/// <summary>
	/// Raised when a new account has been successfully created.
	/// </summary>
	public event EventHandler<AccountCreatedEventArgs>? AccountCreated;

	public AutoCompleteSelector<object> CurrencySelector { get; } = (_, item) => ((Currency)item).Name;

	/// <summary>
	/// Gets a collection of available currencies.
	/// </summary>
	public List<Currency> Currencies { get; }

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
	public Currency? PreferredCurrency
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
	/// Asynchronously creates a new instance of the <see cref="AccountCreationViewModel"/> class.
	/// </summary>
	/// <param name="gnomeshadeClient">API client for getting finance data.</param>
	/// <returns>A new instance of the <see cref="AccountCreationViewModel"/> class.</returns>
	public static async Task<AccountCreationViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
	{
		var currencies = await gnomeshadeClient.GetCurrenciesAsync();
		return new(gnomeshadeClient, currencies);
	}

	/// <summary>
	/// Creates a new account with the given values.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task CreateAccountAsync()
	{
		var accountCreationModel = new AccountCreationModel
		{
			Name = Name,
			PreferredCurrencyId = PreferredCurrency?.Id,
			Currencies = new() { new() { CurrencyId = PreferredCurrency?.Id } },
		};

		var id = await _gnomeshadeClient.CreateAccountAsync(accountCreationModel).ConfigureAwait(true);
		OnAccountCreated(id);
	}

	private void OnAccountCreated(Guid id)
	{
		AccountCreated?.Invoke(this, new(id));
	}
}
