// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using Gnomeshade.WebApi.Models.Owners;

namespace Gnomeshade.Avalonia.Core.Accesses;

/// <summary>A row representing an <see cref="Owner"/>.</summary>
public sealed class OwnerRow : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="OwnerRow"/> class.</summary>
	/// <param name="id">The id of the owner.</param>
	/// <param name="name">The name of the owner.</param>
	/// <param name="ownerships">The ownerships associated with the owner.</param>
	public OwnerRow(Guid id, string name, IReadOnlyCollection<OwnershipRow> ownerships)
	{
		Id = id;
		Name = name;
		Ownerships = ownerships;
	}

	/// <summary>Gets the name of the owner.</summary>
	public string Name { get; }

	/// <summary>Gets the ownerships associated with this owner.</summary>
	public IReadOnlyCollection<OwnershipRow> Ownerships { get; }

	internal Guid Id { get; }
}
