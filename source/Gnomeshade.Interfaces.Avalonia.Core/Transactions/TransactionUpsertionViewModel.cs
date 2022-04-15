// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Controls;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Links;
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

	private TransactionUpsertionViewModel(
		IGnomeshadeClient gnomeshadeClient,
		IDateTimeZoneProvider dateTimeZoneProvider)
		: base(gnomeshadeClient)
	{
		_dateTimeZoneProvider = dateTimeZoneProvider;

		Properties = new();
		Properties.PropertyChanged += PropertiesOnPropertyChanged;
	}

	private TransactionUpsertionViewModel(
		IGnomeshadeClient gnomeshadeClient,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid id,
		TransferViewModel transfers,
		PurchaseViewModel purchases,
		LinkViewModel links)
		: this(gnomeshadeClient, dateTimeZoneProvider)
	{
		_id = id;
		Transfers = transfers;
		Purchases = purchases;
		Links = links;
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

	/// <inheritdoc />
	public override bool CanSave => Properties.IsValid;

	/// <summary>Initializes a new instance of the <see cref="TransactionUpsertionViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="transactionId">The id of the transaction to edit.</param>
	/// <returns>A new instance of the <see cref="TransactionUpsertionViewModel"/> class.</returns>
	public static async Task<TransactionUpsertionViewModel> CreateAsync(
		IGnomeshadeClient gnomeshadeClient,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid? transactionId = null)
	{
		if (transactionId is not { } id)
		{
			return new(gnomeshadeClient, dateTimeZoneProvider);
		}

		var transferViewModel = await TransferViewModel.CreateAsync(gnomeshadeClient, id).ConfigureAwait(false);
		var purchaseViewModel = await PurchaseViewModel.CreateAsync(gnomeshadeClient, dateTimeZoneProvider, id).ConfigureAwait(false);
		var linkViewModel = await LinkViewModel.CreateAsync(gnomeshadeClient, id).ConfigureAwait(false);

		var viewModel = new TransactionUpsertionViewModel(gnomeshadeClient, dateTimeZoneProvider, id, transferViewModel, purchaseViewModel, linkViewModel);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <inheritdoc />
	public override async Task RefreshAsync()
	{
		if (_id is null)
		{
			return;
		}

		var transaction = await GnomeshadeClient.GetTransactionAsync(_id.Value).ConfigureAwait(false);

		Properties.BookingDate = transaction.BookedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset();
		Properties.BookingTime = transaction.BookedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset().TimeOfDay;

		Properties.ValueDate = transaction.ValuedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset();
		Properties.ValueTime = transaction.ValuedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset().TimeOfDay;

		Properties.ReconciliationDate = transaction.ReconciledAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset();
		Properties.ReconciliationTime = transaction.ReconciledAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset().TimeOfDay;

		Properties.Description = transaction.Description;

		if (Transfers is not null)
		{
			await Transfers.RefreshAsync().ConfigureAwait(false);
		}

		if (Purchases is not null)
		{
			await Purchases.RefreshAsync().ConfigureAwait(false);
		}

		if (Links is not null)
		{
			await Links.RefreshAsync().ConfigureAwait(false);
		}
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var creationModel = new TransactionCreationModel
		{
			BookedAt = Properties.BookedAt?.ToInstant(),
			ValuedAt = Properties.ValuedAt?.ToInstant(),
			ReconciledAt = Properties.ReconciledAt?.ToInstant(),
			Description = Properties.Description,
		};

		_id ??= Guid.NewGuid();
		await GnomeshadeClient.PutTransactionAsync(_id.Value, creationModel).ConfigureAwait(false);
		await RefreshAsync().ConfigureAwait(false);

		Transfers ??= await TransferViewModel.CreateAsync(GnomeshadeClient, _id.Value).ConfigureAwait(false);
		Purchases ??= await PurchaseViewModel.CreateAsync(GnomeshadeClient, _dateTimeZoneProvider, _id.Value).ConfigureAwait(false);
		Links ??= await LinkViewModel.CreateAsync(GnomeshadeClient, _id.Value).ConfigureAwait(false);

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
