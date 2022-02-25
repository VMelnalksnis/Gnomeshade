// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;

namespace Gnomeshade.Interfaces.Avalonia.Core.Counterparties;

/// <summary>Single row in counterparty overview.</summary>
public sealed class CounterpartyRow : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="CounterpartyRow"/> class.</summary>
	/// <param name="counterparty">The counterparty this row represents.</param>
	public CounterpartyRow(Counterparty counterparty)
	{
		Id = counterparty.Id;
		Name = counterparty.Name;
	}

	/// <summary>Gets the id of the counterparty.</summary>
	public Guid Id { get; }

	/// <summary>Gets the name of the counterparty.</summary>
	public string Name { get; }
}
