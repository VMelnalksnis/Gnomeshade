// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;

namespace Gnomeshade.Interfaces.Avalonia.Core.Accounts;

/// <summary>Form for viewing and editing a single account.</summary>
public sealed class AccountUpsertionViewModel : UpsertionViewModel
{
	private static readonly string[] _canUpdate = { nameof(CanSave) };

	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Account? _account;

	private string? _name;
	private Counterparty? _counterparty;
	private Currency? _preferredCurrency;
	private string? _bic;
	private string? _iban;
	private string? _accountNumber;
	private Currency? _selectedCurrency;
	private Currency? _addableCurrency;

	private AccountUpsertionViewModel(
		IGnomeshadeClient gnomeshadeClient,
		List<Counterparty> counterparties,
		List<Currency> currencies,
		Account account)
		: this(gnomeshadeClient, counterparties, currencies)
	{
		_account = account;

		_counterparty = Counterparties.Single(counterparty => counterparty.Id == account.CounterpartyId);
		_preferredCurrency = account.PreferredCurrency;
		_name = account.Name;
		_bic = account.Bic;
		_iban = account.Iban;
		_accountNumber = account.AccountNumber;
		AdditionalCurrencies = new(currencies.Where(currency =>
			account.Currencies.Any(c => c.Currency.Id == currency.Id) &&
			_preferredCurrency?.Id != currency.Id));
	}

	private AccountUpsertionViewModel(
		IGnomeshadeClient gnomeshadeClient,
		List<Counterparty> counterparties,
		List<Currency> currencies)
		: base(gnomeshadeClient)
	{
		_gnomeshadeClient = gnomeshadeClient;
		Counterparties = counterparties;
		Currencies = currencies;
		AdditionalCurrencies.CollectionChanged += AdditionalCurrenciesOnCollectionChanged;
	}

	/// <inheritdoc cref="Account.Name"/>
	public string? Name
	{
		get => _name;
		set => SetAndNotifyWithGuard(ref _name, value, nameof(Name), _canUpdate);
	}

	/// <summary>Gets a delegate for formatting a counterparty in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CounterpartySelector => AutoCompleteSelectors.Counterparty;

	/// <summary>Gets a collection of available currencies.</summary>
	public List<Counterparty> Counterparties { get; }

	/// <summary>Gets or sets the preferred current of the account.</summary>
	public Counterparty? Counterparty
	{
		get => _counterparty;
		set => SetAndNotifyWithGuard(ref _counterparty, value, nameof(Counterparty), _canUpdate);
	}

	/// <summary>Gets a delegate for formatting a currency in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CurrencySelector => AutoCompleteSelectors.Currency;

	/// <summary>Gets a collection of available currencies.</summary>
	public List<Currency> Currencies { get; }

	/// <inheritdoc cref="Account.PreferredCurrency"/>
	public Currency? PreferredCurrency
	{
		get => _preferredCurrency;
		set => SetAndNotifyWithGuard(ref _preferredCurrency, value, nameof(PreferredCurrency), nameof(CanSave), nameof(AddableCurrencies));
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

	/// <summary>Gets a collection of all currencies in the account except for <see cref="PreferredCurrency"/>.</summary>
	public ObservableCollection<Currency> AdditionalCurrencies { get; } = new();

	/// <summary>Gets or sets the selected currency from <see cref="AdditionalCurrencies"/>.</summary>
	public Currency? SelectedCurrency
	{
		get => _selectedCurrency;
		set => SetAndNotifyWithGuard(ref _selectedCurrency, value, nameof(SelectedCurrency), nameof(CanRemoveAdditionalCurrency));
	}

	/// <summary>Gets a value indicating whether <see cref="RemoveAdditionalCurrency"/> can be called.</summary>
	public bool CanRemoveAdditionalCurrency => SelectedCurrency is not null;

	/// <summary>Gets a collection of all currencies that can be added as additional currencies.</summary>
	public List<Currency> AddableCurrencies => Currencies
		.Where(currency =>
			AdditionalCurrencies.All(additionalCurrency => additionalCurrency.Id != currency.Id) &&
			currency.Id != PreferredCurrency?.Id)
		.ToList();

	/// <summary>Gets or sets the currency to add to <see cref="AdditionalCurrencies"/>.</summary>
	public Currency? AddableCurrency
	{
		get => _addableCurrency;
		set => SetAndNotifyWithGuard(ref _addableCurrency, value, nameof(AddableCurrency), nameof(CanAddAdditionalCurrency));
	}

	/// <summary>Gets a value indicating whether <see cref="AddAdditionalCurrency"/> can be called.</summary>
	public bool CanAddAdditionalCurrency => AddableCurrency is not null;

	/// <inheritdoc />
	public override bool CanSave =>
		!string.IsNullOrWhiteSpace(Name) &&
		Counterparty is not null &&
		PreferredCurrency is not null;

	/// <summary>Asynchronously creates a new instance of the <see cref="AccountUpsertionViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">API client for getting finance data.</param>
	/// <param name="id">The id of the account to view.</param>
	/// <returns>A new instance of the <see cref="AccountUpsertionViewModel"/> class.</returns>
	public static async Task<AccountUpsertionViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient, Guid? id = null)
	{
		var counterparties = await gnomeshadeClient.GetCounterpartiesAsync().ConfigureAwait(false);
		var currencies = await gnomeshadeClient.GetCurrenciesAsync().ConfigureAwait(false);
		if (id is null)
		{
			return new(gnomeshadeClient, counterparties, currencies);
		}

		var account = await gnomeshadeClient.GetAccountAsync(id.Value).ConfigureAwait(false);
		return new(gnomeshadeClient, counterparties, currencies, account);
	}

	/// <summary>Removes <see cref="SelectedCurrency"/> from <see cref="AddableCurrencies"/>.</summary>
	public void RemoveAdditionalCurrency()
	{
		if (SelectedCurrency is null)
		{
			throw new InvalidOperationException();
		}

		var currencyToRemove = AdditionalCurrencies.Single(currency => currency.Id == SelectedCurrency.Id);
		AdditionalCurrencies.Remove(currencyToRemove);
	}

	/// <summary>Adds <see cref="AddableCurrency"/> to <see cref="AddableCurrencies"/>.</summary>
	public void AddAdditionalCurrency()
	{
		if (AddableCurrency is null)
		{
			throw new InvalidOperationException();
		}

		AdditionalCurrencies.Add(AddableCurrency);
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var currencyIds = new List<Guid>();
		currencyIds.AddRange(AdditionalCurrencies.Select(currency => currency.Id));
		if (!currencyIds.Contains(PreferredCurrency!.Id))
		{
			currencyIds.Add(PreferredCurrency.Id);
		}

		var creationModel = new AccountCreation
		{
			Name = Name,
			Bic = Bic,
			Iban = Iban,
			AccountNumber = AccountNumber,
			PreferredCurrencyId = PreferredCurrency.Id,
			CounterpartyId = Counterparty!.Id,
			Currencies = currencyIds.Select(id => new AccountInCurrencyCreation { CurrencyId = id }).ToList(),
		};

		var id = _account?.Id ?? Guid.NewGuid();
		await _gnomeshadeClient.PutAccountAsync(id, creationModel).ConfigureAwait(false);
		if (_account is not null)
		{
			var missingCurrencies = currencyIds.Where(currencyId => _account.Currencies.All(c => c.Currency.Id != currencyId));
			foreach (var missingCurrency in missingCurrencies)
			{
				await _gnomeshadeClient.AddCurrencyToAccountAsync(id, new() { CurrencyId = missingCurrency });
			}
		}

		return id;
	}

	private void AdditionalCurrenciesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(AddableCurrencies));
	}
}
