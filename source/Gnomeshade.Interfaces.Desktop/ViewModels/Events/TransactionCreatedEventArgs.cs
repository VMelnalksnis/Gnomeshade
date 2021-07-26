// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Desktop.ViewModels.Events
{
	/// <summary>
	/// Event arguments for <see cref="TransactionCreationViewModel.TransactionCreated"/> event.
	/// </summary>
	public sealed class TransactionCreatedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionCreatedEventArgs"/> class.
		/// </summary>
		/// <param name="transactionId">The id of the created transaction.</param>
		public TransactionCreatedEventArgs(Guid transactionId)
		{
			TransactionId = transactionId;
		}

		/// <summary>
		/// Gets the id of the created transaction.
		/// </summary>
		public Guid TransactionId { get; }
	}
}
