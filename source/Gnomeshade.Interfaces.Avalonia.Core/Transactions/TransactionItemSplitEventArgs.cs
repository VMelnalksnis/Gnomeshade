// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

/// <summary>Event arguments for <see cref="TransactionDetailViewModel.ItemSplit"/> event.</summary>
public sealed class TransactionItemSplitEventArgs : EventArgs
{
	/// <summary>Initializes a new instance of the <see cref="TransactionItemSplitEventArgs"/> class.</summary>
	/// <param name="transactionItem">The transaction item to split.</param>
	/// <param name="transactionId">The id of the transaction to edit.</param>
	public TransactionItemSplitEventArgs(TransactionItem transactionItem, Guid transactionId)
	{
		TransactionItem = transactionItem;
		TransactionId = transactionId;
	}

	/// <summary>Gets the transaction item to split.</summary>
	public TransactionItem TransactionItem { get; }

	/// <summary>Gets the id of the transaction to edit.</summary>
	public Guid TransactionId { get; }
}
