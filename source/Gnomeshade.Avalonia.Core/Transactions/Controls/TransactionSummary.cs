// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Gnomeshade.Avalonia.Core.Transactions.Controls;

/// <summary>A summary of a collection of transactions.</summary>
public sealed class TransactionSummary : ViewModelBase
{
	private decimal _received;
	private decimal _withdrawn;

	/// <summary>Gets the total amount deposited into users accounts.</summary>
	public decimal Received
	{
		get => _received;
		private set => SetAndNotifyWithGuard(ref _received, value, nameof(Received), nameof(Total));
	}

	/// <summary>Gets the total amount withdrawn from users accounts.</summary>
	public decimal Withdrawn
	{
		get => _withdrawn;
		private set => SetAndNotifyWithGuard(ref _withdrawn, value, nameof(Withdrawn), nameof(Total));
	}

	/// <summary>Gets the difference between <see cref="Received"/> and <see cref="Withdrawn"/>.</summary>
	public decimal Total => Received - Withdrawn;

	/// <summary>Calculates summary of the specified transactions.</summary>
	/// <param name="transactions">The transactions to summarize.</param>
	public void UpdateTotal(IEnumerable<TransactionOverview> transactions)
	{
		var transfers = transactions
			.SelectMany(transaction => transaction.Transfers)
			.Where(transfer => !transfer.UserToUser)
			.ToList();

		Withdrawn = transfers.Where(transfer => transfer.Direction == "→").Sum(transfer => transfer.UserAmount);
		Received = transfers.Where(transfer => transfer.Direction != "→").Sum(transfer => transfer.UserAmount);
	}
}
