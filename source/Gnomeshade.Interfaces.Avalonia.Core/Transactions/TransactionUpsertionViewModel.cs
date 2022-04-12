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

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

/// <summary>Create or update a transaction.</summary>
public sealed class TransactionUpsertionViewModel : UpsertionViewModel
{
	private readonly Guid _id;

	private TransactionUpsertionViewModel(
		IGnomeshadeClient gnomeshadeClient,
		Guid id,
		TransferViewModel transfers,
		PurchaseViewModel purchases,
		LinkViewModel links)
		: base(gnomeshadeClient)
	{
		_id = id;
		Transfers = transfers;
		Purchases = purchases;
		Links = links;

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
	/// <returns>A new instance of the <see cref="TransactionUpsertionViewModel"/> class.</returns>
	public static async Task<TransactionUpsertionViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient, Guid id)
	{
		var transferViewModel = await TransferViewModel.CreateAsync(gnomeshadeClient, id).ConfigureAwait(false);
		var purchaseViewModel = await PurchaseViewModel.CreateAsync(gnomeshadeClient, id).ConfigureAwait(false);
		var linkViewModel = await LinkViewModel.CreateAsync(gnomeshadeClient, id).ConfigureAwait(false);

		var viewModel = new TransactionUpsertionViewModel(gnomeshadeClient, id, transferViewModel, purchaseViewModel, linkViewModel);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <inheritdoc />
	public override async Task RefreshAsync()
	{
		var transaction = await GnomeshadeClient.GetTransactionAsync(_id).ConfigureAwait(false);

		Properties.BookingDate = transaction.BookedAt?.ToLocalTime();
		Properties.BookingTime = transaction.BookedAt?.ToLocalTime().TimeOfDay;

		Properties.ValueDate = transaction.ValuedAt?.ToLocalTime();
		Properties.ValueTime = transaction.ValuedAt?.ToLocalTime().TimeOfDay;

		Properties.ReconciliationDate = transaction.ReconciledAt?.ToLocalTime();
		Properties.ReconciliationTime = transaction.ReconciledAt?.ToLocalTime().TimeOfDay;

		Properties.Description = transaction.Description;

		await Transfers.RefreshAsync().ConfigureAwait(false);
		await Purchases.RefreshAsync().ConfigureAwait(false);
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var creationModel = new TransactionCreationModel
		{
			BookedAt = Properties.BookedAt,
			ValuedAt = Properties.ValuedAt,
			ReconciledAt = Properties.ReconciledAt,
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
