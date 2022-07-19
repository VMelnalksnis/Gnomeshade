// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Controls;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Links;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Loans;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Purchases;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Transfers;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

/// <summary>Create or update a transaction.</summary>
public sealed class TransactionUpsertionViewModel : UpsertionViewModel
{
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
	private Guid? _id;
	private TransferViewModel? _transfers;
	private PurchaseViewModel? _purchases;
	private LinkViewModel? _links;
	private LoanViewModel? _loans;

	/// <summary>Initializes a new instance of the <see cref="TransactionUpsertionViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="id">The id of the transaction to edit.</param>
	public TransactionUpsertionViewModel(
		IGnomeshadeClient gnomeshadeClient,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid? id)
		: base(gnomeshadeClient)
	{
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_id = id;
		Properties = new();

		Properties.PropertyChanged += PropertiesOnPropertyChanged;
	}

	/// <summary>Gets the transaction information.</summary>
	public TransactionProperties Properties { get; }

	/// <summary>Gets view model of all transfers of this transaction.</summary>
	public TransferViewModel? Transfers
	{
		get => _transfers;
		private set => SetAndNotify(ref _transfers, value);
	}

	/// <summary>Gets view model of all purchases of this transaction.</summary>
	public PurchaseViewModel? Purchases
	{
		get => _purchases;
		private set => SetAndNotify(ref _purchases, value);
	}

	/// <summary>Gets view model of all links of this transaction.</summary>
	public LinkViewModel? Links
	{
		get => _links;
		private set => SetAndNotify(ref _links, value);
	}

	/// <summary>Gets view model of all loans of this transaction.</summary>
	public LoanViewModel? Loans
	{
		get => _loans;
		private set => SetAndNotify(ref _loans, value);
	}

	/// <inheritdoc />
	public override bool CanSave => Properties.IsValid;

	/// <summary>Removed reconciled status from a transaction.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task Edit()
	{
		if (_id is null)
		{
			return;
		}

		var transaction = await GnomeshadeClient.GetTransactionAsync(_id.Value).ConfigureAwait(false);
		if (!transaction.Reconciled)
		{
			return;
		}

		var creation = new TransactionCreation
		{
			BookedAt = transaction.BookedAt,
			Description = transaction.Description,
			ImportedAt = transaction.ImportedAt,
			OwnerId = transaction.OwnerId,
			ReconciledAt = null,
			ValuedAt = transaction.ValuedAt,
		};

		await GnomeshadeClient.PutTransactionAsync(_id.Value, creation).ConfigureAwait(false);
		await RefreshAsync().ConfigureAwait(false);
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		if (_id is null)
		{
			return;
		}

		var transaction = await GnomeshadeClient.GetTransactionAsync(_id.Value).ConfigureAwait(false);

		var defaultZone = _dateTimeZoneProvider.GetSystemDefault();

		Properties.BookingDate = transaction.BookedAt?.InZone(defaultZone).ToDateTimeOffset();
		Properties.BookingTime = transaction.BookedAt?.InZone(defaultZone).ToDateTimeOffset().TimeOfDay;

		Properties.ValueDate = transaction.ValuedAt?.InZone(defaultZone).ToDateTimeOffset();
		Properties.ValueTime = transaction.ValuedAt?.InZone(defaultZone).ToDateTimeOffset().TimeOfDay;

		Properties.ReconciliationDate = transaction.ReconciledAt?.InZone(defaultZone).ToDateTimeOffset();
		Properties.ReconciliationTime = transaction.ReconciledAt?.InZone(defaultZone).ToDateTimeOffset().TimeOfDay;
		Properties.Reconciled = transaction.Reconciled;

		Properties.ImportDate = transaction.ImportedAt?.InZone(defaultZone).ToDateTimeOffset();
		Properties.ImportTime = transaction.ImportedAt?.InZone(defaultZone).ToDateTimeOffset().TimeOfDay;

		Properties.Description = transaction.Description;

		Transfers ??= new(GnomeshadeClient, _id.Value);
		Purchases ??= new(GnomeshadeClient, _dateTimeZoneProvider, _id.Value);
		Links ??= new(GnomeshadeClient, _id.Value);
		Loans ??= new(GnomeshadeClient, _id.Value);

		await Task.WhenAll(
				Transfers.RefreshAsync(),
				Purchases.RefreshAsync(),
				Links.RefreshAsync(),
				Loans.RefreshAsync())
			.ConfigureAwait(false);

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
			BookedAt = Properties.BookedAt?.ToInstant(),
			ValuedAt = Properties.ValuedAt?.ToInstant(),
			ReconciledAt = Properties.ReconciledAt?.ToInstant(),
			ImportedAt = Properties.ImportedAt?.ToInstant(),
			Description = Properties.Description,
		};

		_id ??= Guid.NewGuid();
		await GnomeshadeClient.PutTransactionAsync(_id.Value, creationModel).ConfigureAwait(false);
		await RefreshAsync().ConfigureAwait(false);

		return _id.Value;
	}

	private void PropertiesOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(TransactionProperties.IsValid))
		{
			OnPropertyChanged(nameof(CanSave));
		}
	}
}
