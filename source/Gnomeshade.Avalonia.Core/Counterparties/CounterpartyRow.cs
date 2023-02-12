// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Models.Accounts;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Counterparties;

/// <summary>Single row in counterparty overview.</summary>
public sealed partial class CounterpartyRow : PropertyChangedBase
{
	/// <summary>Gets or sets the balance of all loans issued to and received from the current user.</summary>
	[Notify]
	private decimal _loanBalance;

	/// <summary>Initializes a new instance of the <see cref="CounterpartyRow"/> class.</summary>
	/// <param name="counterparty">The counterparty this row represents.</param>
	/// <param name="loanBalance">The balance of all loans issued to and received from the current user.</param>
	public CounterpartyRow(Counterparty counterparty, decimal loanBalance)
	{
		Id = counterparty.Id;
		Name = counterparty.Name;
		LoanBalance = loanBalance;
	}

	/// <summary>Gets the id of the counterparty.</summary>
	public Guid Id { get; }

	/// <summary>Gets the name of the counterparty.</summary>
	public string Name { get; }
}
