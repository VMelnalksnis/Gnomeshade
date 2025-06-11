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
public sealed partial class TransferUpsertionViewModel : UpsertionViewModel
{
	private readonly IDialogService _dialogService;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
	private readonly Guid _transactionId;

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

	/// <summary>Gets or sets the date on which the transfer was posted to an account on the account servicer accounting books.</summary>
	[Notify]
	private LocalDateTime? _bookingDate;

	/// <summary>Gets or sets the date on which assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	[Notify]
	private LocalDateTime? _valueDate;

	/// <summary>Gets a collection of all active accounts.</summary>
	[Notify(Setter.Private)]
	private List<Account> _accounts = [];

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

	/// <summary>Initializes a new instance of the <see cref="TransferUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="dialogService">Service for creating dialog windows.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="transactionId">The id of the transaction to which to add the transfer to.</param>
	/// <param name="id">The id of the transfer to edit.</param>
	public TransferUpsertionViewModel(
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

	/// <inheritdoc cref="AutoCompleteSelectors.Currency"/>
	public AutoCompleteSelector<object> CurrencySelector => AutoCompleteSelectors.Currency;

	/// <inheritdoc cref="TransferBase.BookedAt"/>
	public ZonedDateTime? BookedAt => BookingDate?.InZoneStrictly(_dateTimeZoneProvider.GetSystemDefault());

	/// <inheritdoc cref="Transfer.ValuedAt"/>
	public ZonedDateTime? ValuedAt => ValueDate?.InZoneStrictly(_dateTimeZoneProvider.GetSystemDefault());

	/// <summary>Gets a value indicating whether <see cref="TargetAmount"/> should not be editable.</summary>
	public bool IsTargetAmountReadOnly => SourceCurrency == TargetCurrency;

	/// <summary>Gets a value indicating whether <see cref="CreateAccount"/> can be invoked.</summary>
	public bool CanCreate => SourceAccount is null || TargetAccount is null;

	/// <inheritdoc />
	public override bool CanSave =>
		SourceAmount is not null &&
		SourceAccount is not null &&
		SourceCurrency is not null &&
		TargetAmount is not null &&
		TargetAccount is not null &&
		TargetCurrency is not null &&
		(BookingDate.HasValue || ValueDate.HasValue);

	/// <summary>Gets a command for showing a dialog for creating a new account.</summary>
	public CommandBase CreateAccount { get; }

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		(Accounts, Currencies) = await
			(GnomeshadeClient.GetAccountsAsync(),
			GnomeshadeClient.GetCurrenciesAsync())
			.WhenAll();

		if (Id is not { } transferId)
		{
			return;
		}

		var transfer = await GnomeshadeClient.GetTransferAsync(transferId);

		SourceAccount = Accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.SourceAccountId));
		SourceAmount = transfer.SourceAmount;
		SourceCurrency = Currencies.Single(cur => cur.Id == SourceAccount.Currencies.Single(c => c.Id == transfer.SourceAccountId).CurrencyId);

		TargetAccount = Accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.TargetAccountId));
		TargetAmount = transfer.TargetAmount;
		TargetCurrency = Currencies.Single(cur => cur.Id == TargetAccount.Currencies.Single(c => c.Id == transfer.TargetAccountId).CurrencyId);

		BankReference = transfer.BankReference;
		ExternalReference = transfer.ExternalReference;
		InternalReference = transfer.InternalReference;

		var defaultZone = _dateTimeZoneProvider.GetSystemDefault();

		BookingDate = transfer.BookedAt?.InZone(defaultZone).LocalDateTime;
		ValueDate = transfer.ValuedAt?.InZone(defaultZone).LocalDateTime;

		Order = transfer.Order;
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var transferCreation = new TransferCreation
		{
			TransactionId = _transactionId,
			SourceAmount = SourceAmount,
			SourceAccountId = SourceAccount!.Currencies.Single(c => c.CurrencyId == SourceCurrency!.Id).Id,
			TargetAmount = TargetAmount,
			TargetAccountId = TargetAccount!.Currencies.Single(c => c.CurrencyId == TargetCurrency!.Id).Id,
			BankReference = BankReference,
			ExternalReference = ExternalReference,
			InternalReference = InternalReference,
			Order = Order,
			BookedAt = BookedAt?.ToInstant(),
			ValuedAt = ValuedAt?.ToInstant(),
		};

		var id = Id ?? Guid.NewGuid();
		await GnomeshadeClient.PutTransferAsync(id, transferCreation);
		return id;
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

		if (e.PropertyName is nameof(SourceAccount))
		{
			if (SourceAccount is null)
			{
				SourceCurrencies = Currencies.ToList();
			}
			else
			{
				var ids = SourceAccount.Currencies.Select(currency => currency.CurrencyId).ToArray();
				SourceCurrencies = Currencies.Where(currency => ids.Contains(currency.Id)).ToList();
			}
		}

		if (e.PropertyName is nameof(TargetAccount))
		{
			if (TargetAccount is null)
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
