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

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;

namespace Gnomeshade.Avalonia.Core.Accounts;

/// <summary>Form for viewing and editing a single account.</summary>
public sealed class AccountUpsertionViewModel : UpsertionViewModel
{
	private static readonly string[] _canUpdate = { nameof(CanSave) };

	private Guid? _id;
	private string? _name;
	private Counterparty? _counterparty;
	private Currency? _preferredCurrency;
	private string? _bic;
	private string? _iban;
	private string? _accountNumber;
	private Currency? _selectedCurrency;
	private Currency? _addableCurrency;
	private List<Counterparty> _counterparties;
	private List<Currency> _currencies;

	/// <summary>Initializes a new instance of the <see cref="AccountUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">API client for getting finance data.</param>
	/// <param name="id">The id of the account to view.</param>
	public AccountUpsertionViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient, Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_id = id;

		_counterparties = new();
		_currencies = new();
		AdditionalCurrencies = new();

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
	public List<Counterparty> Counterparties
	{
		get => _counterparties;
		private set => SetAndNotify(ref _counterparties, value);
	}

	/// <summary>Gets or sets the preferred current of the account.</summary>
	public Counterparty? Counterparty
	{
		get => _counterparty;
		set => SetAndNotifyWithGuard(ref _counterparty, value, nameof(Counterparty), _canUpdate);
	}

	/// <summary>Gets a delegate for formatting a currency in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CurrencySelector => AutoCompleteSelectors.Currency;

	/// <summary>Gets a collection of available currencies.</summary>
	public List<Currency> Currencies
	{
		get => _currencies;
		private set => SetAndNotify(ref _currencies, value);
	}

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
		set => SetAndNotify(ref _bic, value);
	}

	/// <inheritdoc cref="Account.Iban"/>
	public string? Iban
	{
		get => _iban;
		set => SetAndNotify(ref _iban, value);
	}

	/// <inheritdoc cref="Account.AccountNumber"/>
	public string? AccountNumber
	{
		get => _accountNumber;
		set => SetAndNotify(ref _accountNumber, value);
	}

	/// <summary>Gets a collection of all currencies in the account except for <see cref="PreferredCurrency"/>.</summary>
	public ObservableCollection<Currency> AdditionalCurrencies { get; }

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
	protected override async Task Refresh()
	{
		var counterparties = await GnomeshadeClient.GetCounterpartiesAsync();
		var currencies = await GnomeshadeClient.GetCurrenciesAsync();

		Counterparties = counterparties;
		Currencies = currencies;

		if (_id is not { } id)
		{
			return;
		}

		var account = await GnomeshadeClient.GetAccountAsync(id);

		Counterparty = Counterparties.Single(counterparty => counterparty.Id == account.CounterpartyId);
		PreferredCurrency = account.PreferredCurrency;
		Name = account.Name;
		Bic = account.Bic;
		Iban = account.Iban;
		AccountNumber = account.AccountNumber;

		AdditionalCurrencies.Clear();
		var additionalCurrencies = currencies.Where(currency =>
			account.Currencies.Any(c => c.Currency.Id == currency.Id) &&
			_preferredCurrency?.Id != currency.Id);

		foreach (var currency in additionalCurrencies)
		{
			AdditionalCurrencies.Add(currency);
		}
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

		var newAccount = _id is null;
		_id ??= Guid.NewGuid();
		await GnomeshadeClient.PutAccountAsync(_id.Value, creationModel);
		if (newAccount)
		{
			return _id.Value;
		}

		var account = await GnomeshadeClient.GetAccountAsync(_id.Value);
		var missingCurrencies = currencyIds
			.Where(currencyId => account.Currencies.All(c => c.Currency.Id != currencyId));
		foreach (var missingCurrency in missingCurrencies)
		{
			await GnomeshadeClient.AddCurrencyToAccountAsync(_id.Value, new() { CurrencyId = missingCurrency });
		}

		var extraCurrencies = account.Currencies
			.Where(currency => currencyIds.All(id => id != currency.Currency.Id));

		foreach (var extraCurrency in extraCurrencies)
		{
			await GnomeshadeClient.RemoveCurrencyFromAccountAsync(account.Id, extraCurrency.Id);
		}

		return _id.Value;
	}

	private void AdditionalCurrenciesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(AddableCurrencies));
	}
}
