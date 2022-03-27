// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Transfers;

/// <summary>Overview of all <see cref="Transfer"/>s of a single <see cref="Transaction"/>.</summary>
public sealed class TransferViewModel : OverviewViewModel<TransferOverview>
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly Guid _transactionId;

	private TransferViewModel(IGnomeshadeClient gnomeshadeClient, Guid transactionId)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_transactionId = transactionId;
	}

	/// <summary>Initializes a new instance of the <see cref="TransferViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="transactionId">The transaction for which to create a transfer overview.</param>
	/// <returns>A new instance of the <see cref="TransferViewModel"/> class.</returns>
	public static async Task<TransferViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient, Guid transactionId)
	{
		var viewModel = new TransferViewModel(gnomeshadeClient, transactionId);
		await viewModel.RefreshAsync().ConfigureAwait(false);
		return viewModel;
	}

	/// <inheritdoc />
	public override async Task RefreshAsync()
	{
		var transfersTask = _gnomeshadeClient.GetTransfersAsync(_transactionId);
		var accountsTask = _gnomeshadeClient.GetAccountsAsync();
		await Task.WhenAll(transfersTask, accountsTask).ConfigureAwait(false);

		var accounts = accountsTask.Result;
		var overviews = transfersTask.Result.Select(transfer =>
		{
			var sourceAccount = accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.SourceAccountId));
			var sourceCurrency = sourceAccount.Currencies.Single(c => c.Id == transfer.SourceAccountId).Currency;
			var targetAccount = accounts.Single(a => a.Currencies.Any(c => c.Id == transfer.TargetAccountId));
			var targetCurrency = targetAccount.Currencies.Single(c => c.Id == transfer.TargetAccountId).Currency;

			return new TransferOverview(
				transfer.Id,
				transfer.SourceAmount,
				sourceAccount.Name,
				sourceCurrency.AlphabeticCode,
				transfer.TargetAmount,
				targetAccount.Name,
				targetCurrency.AlphabeticCode,
				transfer.BankReference,
				transfer.ExternalReference,
				transfer.InternalReference);
		});

		Rows = new(overviews); // todo sorting
	}
}
