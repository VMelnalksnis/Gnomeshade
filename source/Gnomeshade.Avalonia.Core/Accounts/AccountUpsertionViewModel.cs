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

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Accounts;

/// <summary>Form for viewing and editing a single account.</summary>
public sealed partial class AccountUpsertionViewModel : UpsertionViewModel
{
	/// <summary>Gets a collection of available currencies.</summary>
	[Notify(Setter.Private)]
	private List<Counterparty> _counterparties;

	/// <summary>Gets a collection of available currencies.</summary>
	[Notify(Setter.Private)]
	private List<Currency> _currencies;

	/// <inheritdoc cref="Account.Name"/>
	[Notify]
	private string? _name;

	/// <summary>Gets or sets the preferred current of the account.</summary>
	[Notify]
	private Counterparty? _counterparty;

	/// <inheritdoc cref="Account.PreferredCurrency"/>
	[Notify]
	private Currency? _preferredCurrency;

	/// <inheritdoc cref="Account.Bic"/>
	[Notify]
	private string? _bic;

	/// <inheritdoc cref="Account.Iban"/>
	[Notify]
	private string? _iban;

	/// <inheritdoc cref="Account.AccountNumber"/>
	[Notify]
	private string? _accountNumber;

	/// <summary>Gets or sets the selected currency from <see cref="AdditionalCurrencies"/>.</summary>
	[Notify]
	private Currency? _selectedCurrency;

	/// <summary>Gets or sets the currency to add to <see cref="AdditionalCurrencies"/>.</summary>
	[Notify]
	private Currency? _addableCurrency;

	/// <summary>Initializes a new instance of the <see cref="AccountUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">API client for getting finance data.</param>
	/// <param name="id">The id of the account to view.</param>
	public AccountUpsertionViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient, Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		Id = id;

		_counterparties = new();
		_currencies = new();
		AdditionalCurrencies = new();

		AdditionalCurrencies.CollectionChanged += AdditionalCurrenciesOnCollectionChanged;
	}

	/// <summary>Gets a delegate for formatting a counterparty in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CounterpartySelector => AutoCompleteSelectors.Counterparty;

	/// <summary>Gets a delegate for formatting a currency in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CurrencySelector => AutoCompleteSelectors.Currency;

	/// <summary>Gets a collection of all currencies in the account except for <see cref="PreferredCurrency"/>.</summary>
	public ObservableCollection<Currency> AdditionalCurrencies { get; }

	/// <summary>Gets a collection of all currencies that can be added as additional currencies.</summary>
	public List<Currency> AddableCurrencies => Currencies
		.Where(currency =>
			AdditionalCurrencies.All(additionalCurrency => additionalCurrency.Id != currency.Id) &&
			currency.Id != PreferredCurrency?.Id)
		.ToList();

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

		if (Id is not { } id)
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

		var newAccount = Id is null;
		var id = Id ?? Guid.NewGuid();
		await GnomeshadeClient.PutAccountAsync(id, creationModel);
		if (newAccount)
		{
			return id;
		}

		var account = await GnomeshadeClient.GetAccountAsync(id);
		var missingCurrencies = currencyIds
			.Where(currencyId => account.Currencies.All(c => c.Currency.Id != currencyId));
		foreach (var missingCurrency in missingCurrencies)
		{
			await GnomeshadeClient.AddCurrencyToAccountAsync(id, new() { CurrencyId = missingCurrency });
		}

		var extraCurrencies = account.Currencies
			.Where(currency => currencyIds.All(currencyId => currencyId != currency.Currency.Id));

		foreach (var extraCurrency in extraCurrencies)
		{
			await GnomeshadeClient.RemoveCurrencyFromAccountAsync(account.Id, extraCurrency.Id);
		}

		return id;
	}

	private void AdditionalCurrenciesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(AddableCurrencies));
	}
}
