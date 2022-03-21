// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Avalonia.Core;

/// <summary>Event arguments for <see cref="UpsertionViewModel.Upserted"/> event.</summary>
public sealed class UpsertedEventArgs : EventArgs
{
	/// <summary>Initializes a new instance of the <see cref="UpsertedEventArgs"/> class.</summary>
	/// <param name="id">The id of the upserted entity.</param>
	public UpsertedEventArgs(Guid id)
	{
		Id = id;
	}

	/// <summary>Gets the id of the upserted entity.</summary>
	public Guid Id { get; }
}
