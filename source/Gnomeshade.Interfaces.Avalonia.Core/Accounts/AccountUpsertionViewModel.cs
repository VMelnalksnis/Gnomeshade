// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
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
		set => SetAndNotifyWithGuard(ref _preferredCurrency, value, nameof(PreferredCurrency), _canUpdate);
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

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var currencyIds = new List<Guid>();
		if (_account is not null)
		{
			currencyIds.AddRange(_account.Currencies.Select(currency => currency.Currency.Id));
		}

		if (!currencyIds.Contains(PreferredCurrency!.Id))
		{
			currencyIds.Add(PreferredCurrency.Id);
		}

		var creationModel = new AccountCreationModel
		{
			Name = Name,
			Bic = Bic,
			Iban = Iban,
			AccountNumber = AccountNumber,
			PreferredCurrencyId = PreferredCurrency.Id,
			CounterpartyId = Counterparty!.Id,
			Currencies = currencyIds.Select(id => new AccountInCurrencyCreationModel { CurrencyId = id }).ToList(),
		};

		var id = _account?.Id ?? Guid.NewGuid();
		await _gnomeshadeClient.PutAccountAsync(id, creationModel).ConfigureAwait(false);
		return id;
	}
}
