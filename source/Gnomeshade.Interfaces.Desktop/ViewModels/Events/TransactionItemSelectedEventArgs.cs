// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Desktop.ViewModels.Events
{
	/// <summary>
	/// Event arguments for <see cref="ImportViewModel.TransactionItemSelected"/> event.
	/// </summary>
	public sealed class TransactionItemSelectedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionItemSelectedEventArgs"/> class.
		/// </summary>
		/// <param name="itemId">The id of the selected transaction item.</param>
		public TransactionItemSelectedEventArgs(Guid itemId)
		{
			ItemId = itemId;
		}

		/// <summary>
		/// Gets the id of the selected transaction.
		/// </summary>
		public Guid ItemId { get; }
	}
}
