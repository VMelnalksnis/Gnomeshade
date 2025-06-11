// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core.Accounts;
using Gnomeshade.Avalonia.Core.Commands;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions.Transfers;

/// <summary>Create or update a transfer.</summary>
public sealed partial class PlannedTransferUpsertionViewModel : UpsertionViewModel
{
	private readonly IDialogService _dialogService;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
	private readonly Guid _transactionId;

	/// <summary>Gets or sets the amount withdrawn from <see cref="SourceAccount"/>.</summary>
	[Notify]
	private decimal? _sourceAmount;

	/// <inheritdoc cref="PlannedTransfer.IsSourceAccount"/>
	[Notify]
	private bool _isSourceAccount = true;

	/// <summary>Gets or sets the source account of the transfer.</summary>
	[Notify]
	private Account? _sourceAccount;

	/// <summary>Gets or sets the source counterparty of the transfer.</summary>
	[Notify]
	private Counterparty? _sourceCounterparty;

	/// <summary>Gets or sets the currency of <see cref="SourceAmount"/>.</summary>
	[Notify]
	private Currency? _sourceCurrency;

	/// <summary>Gets or sets the amount deposited to <see cref="TargetAccount"/>.</summary>
	[Notify]
	private decimal? _targetAmount;

	/// <inheritdoc cref="PlannedTransfer.IsTargetAccount"/>
	[Notify]
	private bool _isTargetAccount = true;

	/// <summary>Gets or sets the target account of the transaction item.</summary>
	[Notify]
	private Account? _targetAccount;

	/// <summary>Gets or sets the target counterparty of the transfer.</summary>
	[Notify]
	private Counterparty? _targetCounterparty;

	/// <summary>Gets or sets the currency of <see cref="TargetAmount"/>.</summary>
	[Notify]
	private Currency? _targetCurrency;

	/// <summary>Gets or sets the date on which the transfer was posted to an account on the account servicer accounting books.</summary>
	[Notify]
	private LocalDateTime? _bookingDate;

	/// <summary>Gets a collection of all active accounts.</summary>
	[Notify(Setter.Private)]
	private List<Account> _accounts = [];

	/// <summary>Gets a collection of all active counterparties.</summary>
	[Notify(Setter.Private)]
	private List<Counterparty> _counterparties = [];

	/// <summary>Gets a collection of all currencies.</summary>
	[Notify(Setter.Private)]
	private List<Currency> _currencies = [];

	/// <summary>Gets a collection of currencies available for <see cref="SourceAccount"/>.</summary>
	[Notify(Setter.Private)]
	private List<Currency> _sourceCurrencies = [];

	/// <summary>Gets a collection of currencies available for <see cref="TargetAccount"/>.</summary>
	[Notify(Setter.Private)]
	private List<Currency> _targetCurrencies = [];

	/// <summary>Gets or sets the order of the item within a transaction.</summary>
	[Notify]
	private uint? _order;

	/// <summary>Initializes a new instance of the <see cref="PlannedTransferUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="dialogService">Service for creating dialog windows.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="transactionId">The id of the transaction to which to add the transfer to.</param>
	/// <param name="id">The id of the transfer to edit.</param>
	public PlannedTransferUpsertionViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IDialogService dialogService,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid transactionId,
		Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_dialogService = dialogService;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_transactionId = transactionId;
		Id = id;

		CreateAccount = activityService.Create<Window>(window => ShowAccountDialog(window, null), _ => CanCreate, "Waiting for account creation");
		PropertyChanged += OnPropertyChanged;
	}

	/// <inheritdoc cref="AutoCompleteSelectors.Account"/>
	public AutoCompleteSelector<object> AccountSelector => AutoCompleteSelectors.Account;

	/// <inheritdoc cref="AutoCompleteSelectors.Counterparty"/>
	public AutoCompleteSelector<object> CounterpartySelector => AutoCompleteSelectors.Counterparty;

	/// <inheritdoc cref="AutoCompleteSelectors.Currency"/>
	public AutoCompleteSelector<object> CurrencySelector => AutoCompleteSelectors.Currency;

	/// <summary>Gets a value indicating whether <see cref="TargetAmount"/> should not be editable.</summary>
	public bool IsTargetAmountReadOnly => SourceCurrency == TargetCurrency;

	/// <summary>Gets a value indicating whether <see cref="CreateAccount"/> can be invoked.</summary>
	public bool CanCreate => SourceAccount is null || TargetAccount is null;

	/// <inheritdoc />
	public override bool CanSave =>
		SourceAmount is not null &&
		(IsSourceAccount ? SourceAccount is not null : SourceCounterparty is not null) &&
		SourceCurrency is not null &&
		TargetAmount is not null &&
		(IsTargetAccount ? TargetAccount is not null : TargetCounterparty is not null) &&
		TargetCurrency is not null &&
		BookingDate.HasValue;

	/// <summary>Gets a command for showing a dialog for creating a new account.</summary>
	public CommandBase CreateAccount { get; }

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		(Accounts, Currencies, Counterparties) = await (
			GnomeshadeClient.GetAccountsAsync(),
			GnomeshadeClient.GetCurrenciesAsync(),
			GnomeshadeClient.GetCounterpartiesAsync())
			.WhenAll();

		if (Id is not { } transferId)
		{
			return;
		}

		var transfer = await GnomeshadeClient.GetPlannedTransfer(transferId);
		var defaultZone = _dateTimeZoneProvider.GetSystemDefault();

		SourceAmount = transfer.SourceAmount;
		IsSourceAccount = transfer.IsSourceAccount;
		if (transfer.IsSourceAccount)
		{
			SourceAccount = Accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.SourceAccountId.Value));
			SourceCurrency = Currencies.Single(cur => cur.Id == SourceAccount.Currencies.Single(c => c.Id == transfer.SourceAccountId.Value).CurrencyId);
		}
		else
		{
			SourceCounterparty = Counterparties.Single(counterparty => counterparty.Id == transfer.SourceCounterpartyId.Value);
			SourceCurrency = Currencies.Single(currency => currency.Id == transfer.SourceCurrencyId.Value);
		}

		TargetAmount = transfer.TargetAmount;
		IsTargetAccount = transfer.IsTargetAccount;
		if (transfer.IsTargetAccount)
		{
			TargetAccount = Accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.TargetAccountId.Value));
			TargetCurrency = Currencies.Single(cur => cur.Id == TargetAccount.Currencies.Single(c => c.Id == transfer.TargetAccountId.Value).CurrencyId);
		}
		else
		{
			TargetCounterparty = Counterparties.Single(counterparty => counterparty.Id == transfer.TargetCounterpartyId.Value);
			TargetCurrency = Currencies.Single(currency => currency.Id == transfer.TargetCurrencyId.Value);
		}

		BookingDate = transfer.BookedAt?.InZone(defaultZone).LocalDateTime;
		Order = transfer.Order;
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var transfer = new PlannedTransferCreation
		{
			TransactionId = _transactionId,
			SourceAmount = SourceAmount,
			TargetAmount = TargetAmount,
			Order = Order,
			BookedAt = BookingDate?.InZoneStrictly(_dateTimeZoneProvider.GetSystemDefault()).ToInstant(),
		};

		if (IsSourceAccount)
		{
			transfer.SourceAccountId = SourceAccount?.Currencies.Single(c => c.CurrencyId == SourceCurrency?.Id).Id;
		}
		else
		{
			transfer.SourceCounterpartyId = SourceCounterparty?.Id;
			transfer.SourceCurrencyId = SourceCurrency?.Id;
		}

		if (IsTargetAccount)
		{
			transfer.TargetAccountId = TargetAccount?.Currencies.Single(c => c.CurrencyId == TargetCurrency?.Id).Id;
		}
		else
		{
			transfer.TargetCounterpartyId = TargetCounterparty?.Id;
			transfer.TargetCurrencyId = TargetCurrency?.Id;
		}

		if (Id is { } existingId)
		{
			await GnomeshadeClient.PutPlannedTransfer(existingId, transfer);
		}
		else
		{
			Id = await GnomeshadeClient.CreatePlannedTransfer(transfer);
		}

		return Id.Value;
	}

	private async Task ShowAccountDialog(Window window, Guid? id)
	{
		var viewModel = new AccountUpsertionViewModel(ActivityService, GnomeshadeClient, id);
		await viewModel.RefreshAsync();

		_ = await _dialogService.ShowDialogValue<AccountUpsertionViewModel, Guid>(window, viewModel, dialog =>
		{
			dialog.Title = id.HasValue ? "Edit account" : "Create account";
			viewModel.Upserted += (_, args) => dialog.Close(args.Id);
		});

		await RefreshAsync();
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(CanCreate))
		{
			CreateAccount.InvokeExecuteChanged();
		}

		if (e.PropertyName is nameof(SourceAccount) or nameof(SourceCounterparty) or nameof(IsSourceAccount))
		{
			if (SourceAccount is null || !IsSourceAccount)
			{
				SourceCurrencies = Currencies.ToList();
			}
			else
			{
				var ids = SourceAccount.Currencies.Select(currency => currency.CurrencyId).ToArray();
				SourceCurrencies = Currencies.Where(currency => ids.Contains(currency.Id)).ToList();
			}
		}

		if (e.PropertyName is nameof(TargetAccount) or nameof(TargetCounterparty) or nameof(IsTargetAccount))
		{
			if (TargetAccount is null || !IsTargetAccount)
			{
				TargetCurrencies = Currencies.ToList();
			}
			else
			{
				var ids = TargetAccount.Currencies.Select(currency => currency.CurrencyId).ToArray();
				TargetCurrencies = Currencies.Where(currency => ids.Contains(currency.Id)).ToList();
			}
		}

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
