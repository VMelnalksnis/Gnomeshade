// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Items;

/// <summary>Event arguments for <see cref="TransactionViewModel.TransactionSelected"/> event.</summary>
public sealed class TransactionSelectedEventArgs : EventArgs
{
	/// <summary>Initializes a new instance of the <see cref="TransactionSelectedEventArgs"/> class.</summary>
	/// <param name="transactionId">The id of the selected transaction.</param>
	public TransactionSelectedEventArgs(Guid transactionId)
	{
		TransactionId = transactionId;
	}

	/// <summary>Gets the id of the selected transaction.</summary>
	public Guid TransactionId { get; }
}
