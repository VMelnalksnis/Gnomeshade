// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

/// <summary>Create or update a transfer.</summary>
public sealed class TransferUpsertionViewModel : UpsertionViewModel
{
	private static readonly string[] _targetAmountNames = CanSaveNames.Append(nameof(IsTargetAmountReadOnly)).ToArray();

	private readonly Guid _transactionId;
	private readonly Transfer? _transfer;

	private decimal? _sourceAmount;
	private Account? _sourceAccount;
	private Currency? _sourceCurrency;
	private decimal? _targetAmount;
	private Account? _targetAccount;
	private Currency? _targetCurrency;
	private string? _bankReference;
	private string? _externalReference;
	private string? _internalReference;

	private TransferUpsertionViewModel(
		IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		List<Account> accounts,
		List<Currency> currencies)
		: base(gnomeshadeClient)
	{
		_transactionId = transactionId;
		Accounts = accounts;
		Currencies = currencies;

		AccountSelector = (_, item) => ((Account)item).Name;
		CurrencySelector = (_, item) => ((Currency)item).AlphabeticCode;

		PropertyChanged += OnPropertyChanged;
	}

	private TransferUpsertionViewModel(
		IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		List<Account> accounts,
		List<Currency> currencies,
		Transfer transfer)
		: this(gnomeshadeClient, transactionId, accounts, currencies)
	{
		_transfer = transfer;

		SourceAccount = Accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.SourceAccountId));
		SourceAmount = transfer.SourceAmount;
		SourceCurrency = SourceAccount.Currencies.Single(c => c.Id == transfer.SourceAccountId).Currency;

		TargetAccount = Accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.TargetAccountId));
		TargetAmount = transfer.TargetAmount;
		TargetCurrency = TargetAccount.Currencies.Single(c => c.Id == transfer.TargetAccountId).Currency;

		BankReference = transfer.BankReference;
		ExternalReference = transfer.ExternalReference;
		InternalReference = transfer.InternalReference;
	}

	/// <summary>Gets a collection of all active accounts.</summary>
	public List<Account> Accounts { get; }

	/// <summary>Gets a collection of all currencies.</summary>
	public List<Currency> Currencies { get; }

	/// <summary>Gets a delegate for formatting an account in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> AccountSelector { get; }

	/// <summary>Gets a delegate for formatting a currency in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CurrencySelector { get; }

	/// <summary>Gets or sets the source account of the transaction item.</summary>
	public Account? SourceAccount
	{
		get => _sourceAccount;
		set => SetAndNotifyWithGuard(ref _sourceAccount, value, nameof(SourceAccount), CanSaveNames);
	}

	/// <summary>Gets or sets the amount withdrawn from <see cref="SourceAccount"/>.</summary>
	public decimal? SourceAmount
	{
		get => _sourceAmount;
		set => SetAndNotifyWithGuard(ref _sourceAmount, value, nameof(SourceAmount), CanSaveNames);
	}

	/// <summary>Gets or sets the currency of <see cref="SourceAmount"/>.</summary>
	public Currency? SourceCurrency
	{
		get => _sourceCurrency;
		set => SetAndNotifyWithGuard(ref _sourceCurrency, value, nameof(SourceCurrency), _targetAmountNames);
	}

	/// <summary>Gets or sets the target account of the transaction item.</summary>
	public Account? TargetAccount
	{
		get => _targetAccount;
		set => SetAndNotifyWithGuard(ref _targetAccount, value, nameof(TargetAccount), CanSaveNames);
	}

	/// <summary>Gets or sets the amount deposited to <see cref="TargetAccount"/>.</summary>
	public decimal? TargetAmount
	{
		get => _targetAmount;
		set => SetAndNotifyWithGuard(ref _targetAmount, value, nameof(TargetAmount), CanSaveNames);
	}

	/// <summary>Gets or sets the currency of <see cref="TargetAmount"/>.</summary>
	public Currency? TargetCurrency
	{
		get => _targetCurrency;
		set => SetAndNotifyWithGuard(ref _targetCurrency, value, nameof(TargetCurrency), _targetAmountNames);
	}

	/// <summary>Gets or sets the bank reference of the transaction item.</summary>
	public string? BankReference
	{
		get => _bankReference;
		set => SetAndNotify(ref _bankReference, value, nameof(BankReference));
	}

	/// <summary>Gets or sets the external reference of the transaction item.</summary>
	public string? ExternalReference
	{
		get => _externalReference;
		set => SetAndNotify(ref _externalReference, value, nameof(ExternalReference));
	}

	/// <summary>Gets or sets the internal reference of the transaction item.</summary>
	public string? InternalReference
	{
		get => _internalReference;
		set => SetAndNotify(ref _internalReference, value, nameof(InternalReference));
	}

	/// <summary>Gets a value indicating whether <see cref="TargetAmount"/> should not be editable.</summary>
	public bool IsTargetAmountReadOnly => SourceCurrency == TargetCurrency;

	/// <inheritdoc />
	public override bool CanSave =>
		SourceAmount is not null &&
		SourceAccount is not null &&
		SourceCurrency is not null &&
		TargetAmount is not null &&
		TargetAccount is not null &&
		TargetCurrency is not null;

	/// <summary>Initializes a new instance of the <see cref="TransferUpsertionViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="transactionId">The id of the transaction to which to add the transfer to.</param>
	/// <param name="id">The id of the transfer to edit.</param>
	/// <returns>A new instance of the <see cref="TransferUpsertionViewModel"/> class.</returns>
	public static async Task<TransferUpsertionViewModel> CreateAsync(
		IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid? id = null)
	{
		if (id is null)
		{
			var accountsTask = gnomeshadeClient.GetAccountsAsync();
			var currenciesTask = gnomeshadeClient.GetCurrenciesAsync();

			await Task.WhenAll(accountsTask, currenciesTask).ConfigureAwait(false);
			return new(gnomeshadeClient, transactionId, accountsTask.Result, currenciesTask.Result);
		}
		else
		{
			var accountsTask = gnomeshadeClient.GetAccountsAsync();
			var currenciesTask = gnomeshadeClient.GetCurrenciesAsync();
			var transferTask = gnomeshadeClient.GetTransferAsync(transactionId, id.Value);

			await Task.WhenAll(accountsTask, currenciesTask, transferTask).ConfigureAwait(false);
			return new(gnomeshadeClient, transactionId, accountsTask.Result, currenciesTask.Result, transferTask.Result);
		}
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var transferCreation = new TransferCreation
		{
			SourceAmount = SourceAmount,
			SourceAccountId = SourceAccount!.Currencies.Single(c => c.Currency.Id == SourceCurrency!.Id).Id,
			TargetAmount = TargetAmount,
			TargetAccountId = TargetAccount!.Currencies.Single(c => c.Currency.Id == TargetCurrency!.Id).Id,
			BankReference = BankReference,
			ExternalReference = ExternalReference,
			InternalReference = InternalReference,
		};

		var id = _transfer?.Id ?? Guid.NewGuid();
		await GnomeshadeClient.PutTransferAsync(_transactionId, id, transferCreation).ConfigureAwait(false);
		return id;
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (IsTargetAmountReadOnly && e.PropertyName is nameof(SourceAmount))
		{
			TargetAmount = SourceAmount;
		}
	}
}
