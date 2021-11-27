// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Desktop.ViewModels.Events;

/// <summary>
/// Event arguments for <see cref="TransactionItemCreationViewModel.TransactionItemCreated"/> event.
/// </summary>
public sealed class TransactionItemCreatedEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TransactionItemCreatedEventArgs"/> class.
	/// </summary>
	/// <param name="itemId">The id of the created transaction item.</param>
	public TransactionItemCreatedEventArgs(Guid itemId)
	{
		ItemId = itemId;
	}

	/// <summary>
	/// Gets the id of the created transaction item.
	/// </summary>
	public Guid ItemId { get; }
}
