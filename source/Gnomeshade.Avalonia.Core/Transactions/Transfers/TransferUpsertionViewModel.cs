// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.Avalonia.Core.Transactions.Transfers;

/// <summary>Create or update a transfer.</summary>
public sealed class TransferUpsertionViewModel : UpsertionViewModel
{
	private static readonly string[] _targetAmountNames = CanSaveNames.Append(nameof(IsTargetAmountReadOnly)).ToArray();

	private readonly Guid _transactionId;
	private readonly Guid? _id;

	private decimal? _sourceAmount;
	private Account? _sourceAccount;
	private Currency? _sourceCurrency;
	private decimal? _targetAmount;
	private Account? _targetAccount;
	private Currency? _targetCurrency;
	private string? _bankReference;
	private string? _externalReference;
	private string? _internalReference;
	private List<Account> _accounts;
	private List<Currency> _currencies;
	private uint? _order;

	/// <summary>Initializes a new instance of the <see cref="TransferUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="transactionId">The id of the transaction to which to add the transfer to.</param>
	/// <param name="id">The id of the transfer to edit.</param>
	public TransferUpsertionViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient, Guid transactionId, Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_transactionId = transactionId;
		_id = id;

		_accounts = new();
		_currencies = new();

		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Gets a collection of all active accounts.</summary>
	public List<Account> Accounts
	{
		get => _accounts;
		private set => SetAndNotify(ref _accounts, value);
	}

	/// <summary>Gets a collection of all currencies.</summary>
	public List<Currency> Currencies
	{
		get => _currencies;
		private set => SetAndNotify(ref _currencies, value);
	}

	/// <summary>Gets a delegate for formatting an account in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> AccountSelector => AutoCompleteSelectors.Account;

	/// <summary>Gets a delegate for formatting a currency in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CurrencySelector => AutoCompleteSelectors.Currency;

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
		set => SetAndNotify(ref _bankReference, value);
	}

	/// <summary>Gets or sets the external reference of the transaction item.</summary>
	public string? ExternalReference
	{
		get => _externalReference;
		set => SetAndNotify(ref _externalReference, value);
	}

	/// <summary>Gets or sets the internal reference of the transaction item.</summary>
	public string? InternalReference
	{
		get => _internalReference;
		set => SetAndNotify(ref _internalReference, value);
	}

	/// <summary>Gets or sets the order of the item within a transaction.</summary>
	public uint? Order
	{
		get => _order;
		set => SetAndNotify(ref _order, value);
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

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var accountsTask = GnomeshadeClient.GetAccountsAsync();
		var currenciesTask = GnomeshadeClient.GetCurrenciesAsync();

		await Task.WhenAll(accountsTask, currenciesTask);

		Accounts = accountsTask.Result;
		Currencies = currenciesTask.Result;

		if (_id is null)
		{
			return;
		}

		var transfer = await GnomeshadeClient.GetTransferAsync(_id.Value);

		SourceAccount = Accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.SourceAccountId));
		SourceAmount = transfer.SourceAmount;
		SourceCurrency = SourceAccount.Currencies.Single(c => c.Id == transfer.SourceAccountId).Currency;

		TargetAccount = Accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.TargetAccountId));
		TargetAmount = transfer.TargetAmount;
		TargetCurrency = TargetAccount.Currencies.Single(c => c.Id == transfer.TargetAccountId).Currency;

		BankReference = transfer.BankReference;
		ExternalReference = transfer.ExternalReference;
		InternalReference = transfer.InternalReference;

		Order = transfer.Order;
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var transferCreation = new TransferCreation
		{
			TransactionId = _transactionId,
			SourceAmount = SourceAmount,
			SourceAccountId = SourceAccount!.Currencies.Single(c => c.Currency.Id == SourceCurrency!.Id).Id,
			TargetAmount = TargetAmount,
			TargetAccountId = TargetAccount!.Currencies.Single(c => c.Currency.Id == TargetCurrency!.Id).Id,
			BankReference = BankReference,
			ExternalReference = ExternalReference,
			InternalReference = InternalReference,
			Order = Order,
		};

		var id = _id ?? Guid.NewGuid();
		await GnomeshadeClient.PutTransferAsync(id, transferCreation);
		return id;
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (!IsTargetAmountReadOnly)
		{
			return;
		}

		switch (e.PropertyName)
		{
			case nameof(SourceAmount):
				TargetAmount = SourceAmount;
				return;

			case nameof(SourceCurrency):
				TargetCurrency = SourceCurrency;
				return;
		}
	}
}
