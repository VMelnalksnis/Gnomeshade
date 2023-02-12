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

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions.Transfers;

/// <summary>Create or update a transfer.</summary>
public sealed partial class TransferUpsertionViewModel : UpsertionViewModel
{
	private readonly Guid _transactionId;
	private readonly Guid? _id;

	/// <summary>Gets or sets the amount withdrawn from <see cref="SourceAccount"/>.</summary>
	[Notify]
	private decimal? _sourceAmount;

	/// <summary>Gets or sets the source account of the transaction item.</summary>
	[Notify]
	private Account? _sourceAccount;

	/// <summary>Gets or sets the currency of <see cref="SourceAmount"/>.</summary>
	[Notify]
	private Currency? _sourceCurrency;

	/// <summary>Gets or sets the amount deposited to <see cref="TargetAccount"/>.</summary>
	[Notify]
	private decimal? _targetAmount;

	/// <summary>Gets or sets the target account of the transaction item.</summary>
	[Notify]
	private Account? _targetAccount;

	/// <summary>Gets or sets the currency of <see cref="TargetAmount"/>.</summary>
	[Notify]
	private Currency? _targetCurrency;

	/// <summary>Gets or sets the bank reference of the transaction item.</summary>
	[Notify]
	private string? _bankReference;

	/// <summary>Gets or sets the external reference of the transaction item.</summary>
	[Notify]
	private string? _externalReference;

	/// <summary>Gets or sets the internal reference of the transaction item.</summary>
	[Notify]
	private string? _internalReference;

	/// <summary>Gets a collection of all active accounts.</summary>
	[Notify(Setter.Private)]
	private List<Account> _accounts;

	/// <summary>Gets a collection of all currencies.</summary>
	[Notify(Setter.Private)]
	private List<Currency> _currencies;

	/// <summary>Gets or sets the order of the item within a transaction.</summary>
	[Notify]
	private uint? _order;

	/// <summary>Initializes a new instance of the <see cref="TransferUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="transactionId">The id of the transaction to which to add the transfer to.</param>
	/// <param name="id">The id of the transfer to edit.</param>
	public TransferUpsertionViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		Guid transactionId,
		Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_transactionId = transactionId;
		_id = id;

		_accounts = new();
		_currencies = new();

		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Gets a delegate for formatting an account in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> AccountSelector => AutoCompleteSelectors.Account;

	/// <summary>Gets a delegate for formatting a currency in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> CurrencySelector => AutoCompleteSelectors.Currency;

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
