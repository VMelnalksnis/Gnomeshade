// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Commands;
using Gnomeshade.Avalonia.Core.Transactions.Controls;
using Gnomeshade.Avalonia.Core.Transactions.Links;
using Gnomeshade.Avalonia.Core.Transactions.Loans;
using Gnomeshade.Avalonia.Core.Transactions.Purchases;
using Gnomeshade.Avalonia.Core.Transactions.Transfers;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions;

/// <summary>Create or update a transaction.</summary>
public sealed partial class TransactionUpsertionViewModel : UpsertionViewModel
{
	private readonly ICommandFactory _commandFactory;
	private readonly IDialogService _dialogService;
	private readonly IClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	/// <summary>Gets view model of all transfers of this transaction.</summary>
	[Notify(Setter.Private)]
	private TransferViewModel? _transfers;

	/// <summary>Gets view model of all purchases of this transaction.</summary>
	[Notify(Setter.Private)]
	private PurchaseViewModel? _purchases;

	/// <summary>Gets view model of all links of this transaction.</summary>
	[Notify(Setter.Private)]
	private LinkViewModel? _links;

	/// <summary>Gets view model of all loans of this transaction.</summary>
	[Notify(Setter.Private)]
	private LoanPaymentViewModel? _loans;

	/// <summary>Initializes a new instance of the <see cref="TransactionUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="commandFactory">Service for creating commands.</param>
	/// <param name="dialogService">Service for creating dialog windows.</param>
	/// <param name="clock">Clock which can provide the current instant.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="id">The id of the transaction to edit.</param>
	public TransactionUpsertionViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		ICommandFactory commandFactory,
		IDialogService dialogService,
		IClock clock,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_commandFactory = commandFactory;
		_dialogService = dialogService;
		_clock = clock;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		Id = id;
		Properties = new(activityService);

		Properties.PropertyChanged += PropertiesOnPropertyChanged;
	}

	/// <summary>Gets the transaction information.</summary>
	public TransactionProperties Properties { get; }

	/// <inheritdoc />
	public override bool CanSave => Properties.IsValid;

	/// <summary>Marks the current transaction as reconciled at the current time.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task Reconcile()
	{
		if (!CanSave)
		{
			return;
		}

		var currentZone = _dateTimeZoneProvider.GetSystemDefault();
		var currentTime = _clock.GetCurrentInstant().InZone(currentZone).LocalDateTime;
		Properties.ReconciliationDate = currentTime;

		await SaveAsync();
	}

	/// <summary>Removed reconciled status from a transaction.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task Edit()
	{
		if (Id is not { } transactionId)
		{
			return;
		}

		var transaction = await GnomeshadeClient.GetTransactionAsync(transactionId);
		if (!transaction.Reconciled)
		{
			return;
		}

		var creation = new TransactionCreation
		{
			Description = transaction.Description,
			ImportedAt = transaction.ImportedAt,
			OwnerId = transaction.OwnerId,
			ReconciledAt = null,
		};

		await GnomeshadeClient.PutTransactionAsync(transactionId, creation);
		await RefreshAsync();
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		if (Id is not { } transactionId)
		{
			return;
		}

		var transaction = await GnomeshadeClient.GetTransactionAsync(transactionId);

		var defaultZone = _dateTimeZoneProvider.GetSystemDefault();

		Properties.ReconciliationDate = transaction.ReconciledAt?.InZone(defaultZone).LocalDateTime;
		Properties.Reconciled = transaction.Reconciled;

		Properties.ImportDate = transaction.ImportedAt?.InZone(defaultZone).LocalDateTime;

		if (transaction.RefundedBy is { } refundId)
		{
			var refundTransaction = await GnomeshadeClient.GetDetailedTransactionAsync(refundId);
			var refundTime = (refundTransaction.ValuedAt ?? refundTransaction.BookedAt)!.Value;

			Properties.RefundDate = refundTime.InZone(defaultZone).LocalDateTime;
		}

		Properties.Description = transaction.Description;

		var owners = await GnomeshadeClient.GetOwnersAsync();
		Properties.Owners = owners;
		Properties.Owner = owners.SingleOrDefault(owner => owner.Id == transaction.OwnerId);

		Transfers ??= new(ActivityService, GnomeshadeClient, _commandFactory, _dialogService, _dateTimeZoneProvider, transactionId);
		Purchases ??= new(ActivityService, GnomeshadeClient, _commandFactory, _dialogService, _dateTimeZoneProvider, transactionId);
		Links ??= new(ActivityService, GnomeshadeClient, transactionId);
		Loans ??= new(ActivityService, GnomeshadeClient, transactionId);

		await Task.WhenAll(
			Transfers.RefreshAsync(),
			Purchases.RefreshAsync(),
			Links.RefreshAsync(),
			Loans.RefreshAsync());

		if (!transaction.Reconciled)
		{
			return;
		}

		if (!Links.Rows.Any())
		{
			Links = null;
		}

		if (!Loans.Rows.Any())
		{
			Loans = null;
		}
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var creationModel = new TransactionCreation
		{
			ReconciledAt = Properties.ReconciledAt?.ToInstant(),
			ImportedAt = Properties.ImportedAt?.ToInstant(),
			Description = Properties.Description,
			OwnerId = Properties.Owner?.Id,
		};

		var id = Id ?? Guid.NewGuid();
		await GnomeshadeClient.PutTransactionAsync(id, creationModel);
		await RefreshAsync();

		return id;
	}

	private void PropertiesOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(TransactionProperties.IsValid))
		{
			OnPropertyChanged(nameof(CanSave));
		}
	}
}
