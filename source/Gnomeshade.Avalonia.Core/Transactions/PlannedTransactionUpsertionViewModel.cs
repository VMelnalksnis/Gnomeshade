// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Transactions.Loans;
using Gnomeshade.Avalonia.Core.Transactions.Purchases;
using Gnomeshade.Avalonia.Core.Transactions.Transfers;
using Gnomeshade.WebApi.Client;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions;

/// <summary>Create or update a planned transaction.</summary>
public sealed partial class PlannedTransactionUpsertionViewModel : TransactionUpsertionBase
{
	private readonly IDialogService _dialogService;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	/// <summary>Gets view model of all transfers of this transaction.</summary>
	[Notify(Setter.Private)]
	private PlannedTransferViewModel? _transfers;

	/// <summary>Gets view model of all purchases of this transaction.</summary>
	[Notify(Setter.Private)]
	private PlannedPurchaseViewModel? _purchases;

	/// <summary>Gets view model of all loans of this transaction.</summary>
	[Notify(Setter.Private)]
	private PlannedLoanPaymentViewModel? _loans;

	/// <summary>Initializes a new instance of the <see cref="PlannedTransactionUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="dialogService">Service for creating dialog windows.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="id">The id of the transaction to edit.</param>
	public PlannedTransactionUpsertionViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IDialogService dialogService,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid? id)
		: base(activityService, gnomeshadeClient, id)
	{
		_dialogService = dialogService;
		_dateTimeZoneProvider = dateTimeZoneProvider;
	}

	/// <inheritdoc />
	public override bool CanSave => false;

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		if (Id is not { } transactionId)
		{
			IsReadOnly = false;
			return;
		}

		Transfers ??= new(ActivityService, GnomeshadeClient, _dialogService, _dateTimeZoneProvider, transactionId);
		Purchases ??= new(ActivityService, GnomeshadeClient, _dialogService, _dateTimeZoneProvider, transactionId);
		Loans ??= new(ActivityService, GnomeshadeClient, transactionId);

		await Task.WhenAll(
			Transfers.RefreshAsync(),
			Purchases.RefreshAsync(),
			Loans.RefreshAsync());

		if (!Loans.Rows.Any())
		{
			Loans = null;
		}
	}

	/// <inheritdoc />
	protected override Task<Guid> SaveValidatedAsync()
	{
		throw new NotImplementedException();
	}
}
