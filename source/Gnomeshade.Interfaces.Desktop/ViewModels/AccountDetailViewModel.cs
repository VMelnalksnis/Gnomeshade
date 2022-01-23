// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Interfaces.Desktop.ViewModels.Design;
using Gnomeshade.Interfaces.Desktop.Views;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;

namespace Gnomeshade.Interfaces.Desktop.ViewModels;

/// <summary>
/// Form for viewing and editing a single account.
/// </summary>
public sealed class AccountDetailViewModel : ViewModelBase<AccountDetailView>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Account _account;

	private Currency _preferredCurrency;
	private string _name;
	private string? _bic;
	private string? _iban;
	private string? _accountNumber;

	private AccountDetailViewModel(IGnomeshadeClient gnomeshadeClient, Account account, List<Currency> currencies)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_account = account;
		Currencies = currencies;

		_preferredCurrency = account.PreferredCurrency;
		_name = account.Name;
		_bic = account.Bic;
		_iban = account.Iban;
		_accountNumber = account.AccountNumber;
	}

	public AutoCompleteSelector<object> CurrencySelector { get; } = (_, item) => ((Currency)item).Name;

	/// <summary>
	/// Gets a collection of available currencies.
	/// </summary>
	public List<Currency> Currencies { get; }

	/// <summary>
	/// Gets or sets the preferred current of the account.
	/// </summary>
	public Currency PreferredCurrency
	{
		get => _preferredCurrency;
		set => SetAndNotifyWithGuard(ref _preferredCurrency, value, nameof(PreferredCurrency));
	}

	/// <inheritdoc cref="Account.Name"/>
	public string Name
	{
		get => _name;
		set => SetAndNotifyWithGuard(ref _name, value, nameof(Name), nameof(CanUpdate));
	}

	/// <inheritdoc cref="Account.Bic"/>
	public string? Bic
	{
		get => _bic;
		set => SetAndNotify(ref _bic, value, nameof(Bic));
	}

	/// <inheritdoc cref="Account.Iban"/>
	public string? Iban
	{
		get => _iban;
		set => SetAndNotify(ref _iban, value, nameof(Iban));
	}

	/// <inheritdoc cref="Account.AccountNumber"/>
	public string? AccountNumber
	{
		get => _accountNumber;
		set => SetAndNotify(ref _accountNumber, value, nameof(AccountNumber));
	}

	/// <summary>
	/// Gets a value indicating whether or not the account can be updated.
	/// </summary>
	public bool CanUpdate => !string.IsNullOrWhiteSpace(Name);

	/// <summary>
	/// Asynchronously creates a new instance of the <see cref="AccountDetailViewModel"/> class.
	/// </summary>
	/// <param name="gnomeshadeClient">API client for getting finance data.</param>
	/// <param name="id">The id of the account to view.</param>
	/// <returns>A new instance of the <see cref="AccountDetailViewModel"/> class.</returns>
	public static async Task<AccountDetailViewModel> CreateAsync(DesignTimeGnomeshadeClient gnomeshadeClient, Guid id)
	{
		var account = await gnomeshadeClient.GetAccountAsync(id);
		var currencies = await gnomeshadeClient.GetCurrenciesAsync();

		return new(gnomeshadeClient, account, currencies);
	}

	/// <summary>
	/// Updates the selected account with the given values.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task UpdateAccountAsync()
	{
		var creationModel = new AccountCreationModel
		{
			Name = Name,
			Bic = Bic,
			Iban = Iban,
			AccountNumber = AccountNumber,
		};

		await _gnomeshadeClient.PutAccountAsync(_account.Id, creationModel);
	}
}
