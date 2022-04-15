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
	private readonly Guid _id;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private TransactionUpsertionViewModel(
		IGnomeshadeClient gnomeshadeClient,
		Guid id,
		TransferViewModel transfers,
		PurchaseViewModel purchases,
		LinkViewModel links,
		IDateTimeZoneProvider dateTimeZoneProvider)
		: base(gnomeshadeClient)
	{
		_id = id;
		Transfers = transfers;
		Purchases = purchases;
		Links = links;
		_dateTimeZoneProvider = dateTimeZoneProvider;

		Properties = new();
		Properties.PropertyChanged += PropertiesOnPropertyChanged;
	}

	/// <summary>Gets the transaction information.</summary>
	public TransactionProperties Properties { get; }

	/// <summary>Gets view model of all transfers of this transaction.</summary>
	public TransferViewModel Transfers { get; }

	/// <summary>Gets view model of all purchases of this transaction.</summary>
	public PurchaseViewModel Purchases { get; }

	/// <summary>Gets view model of all links of this transaction.</summary>
	public LinkViewModel Links { get; }

	/// <inheritdoc />
	public override bool CanSave => Properties.IsValid;

	/// <summary>Initializes a new instance of the <see cref="TransactionUpsertionViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="id">The id of the transaction to edit.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <returns>A new instance of the <see cref="TransactionUpsertionViewModel"/> class.</returns>
	public static async Task<TransactionUpsertionViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient, Guid id, IDateTimeZoneProvider dateTimeZoneProvider)
	{
		var transferViewModel = await TransferViewModel.CreateAsync(gnomeshadeClient, id).ConfigureAwait(false);
		var purchaseViewModel = await PurchaseViewModel.CreateAsync(gnomeshadeClient, id).ConfigureAwait(false);
		var linkViewModel = await LinkViewModel.CreateAsync(gnomeshadeClient, id).ConfigureAwait(false);

		var viewModel = new TransactionUpsertionViewModel(gnomeshadeClient, id, transferViewModel, purchaseViewModel, linkViewModel, dateTimeZoneProvider);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <inheritdoc />
	public override async Task RefreshAsync()
	{
		var transaction = await GnomeshadeClient.GetTransactionAsync(_id).ConfigureAwait(false);

		Properties.BookingDate = transaction.BookedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset();
		Properties.BookingTime = transaction.BookedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset().TimeOfDay;

		Properties.ValueDate = transaction.ValuedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset();
		Properties.ValueTime = transaction.ValuedAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset().TimeOfDay;

		Properties.ReconciliationDate = transaction.ReconciledAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset();
		Properties.ReconciliationTime = transaction.ReconciledAt?.InZone(_dateTimeZoneProvider.GetSystemDefault()).ToDateTimeOffset().TimeOfDay;

		Properties.Description = transaction.Description;

		await Transfers.RefreshAsync().ConfigureAwait(false);
		await Purchases.RefreshAsync().ConfigureAwait(false);
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

		await GnomeshadeClient.PutTransactionAsync(_id, creationModel).ConfigureAwait(false);
		await RefreshAsync().ConfigureAwait(false);
		return _id;
	}

	private void PropertiesOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(TransactionProperties.IsValid))
		{
			OnPropertyChanged(nameof(CanSave));
		}
	}
}
