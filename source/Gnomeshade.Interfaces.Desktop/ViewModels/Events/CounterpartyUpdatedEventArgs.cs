// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Desktop.ViewModels.Events;

/// <summary>Event arguments for <see cref="CounterpartyUpdateViewModel.Updated"/> event.</summary>
public sealed class CounterpartyUpdatedEventArgs : EventArgs
{
	/// <summary>Initializes a new instance of the <see cref="CounterpartyUpdatedEventArgs"/> class.</summary>
	/// <param name="id">The id of the updated counterparty.</param>
	public CounterpartyUpdatedEventArgs(Guid id)
	{
		Id = id;
	}

	/// <summary>Gets the id of the updated counterparty.</summary>
	public Guid Id { get; }
}
