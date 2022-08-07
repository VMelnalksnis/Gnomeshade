// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Avalonia.Core.Accounts;

/// <summary>Single row in account overview.</summary>
public sealed class AccountOverviewRow : PropertyChangedBase
{
	private decimal _balance;

	/// <summary>Initializes a new instance of the <see cref="AccountOverviewRow"/> class.</summary>
	/// <param name="id">The id of the account.</param>
	/// <param name="name">The name of the account.</param>
	/// <param name="currency">The currency of the account.</param>
	/// <param name="balance">The balance of the account.</param>
	/// <param name="disabled">Whether this account is disabled.</param>
	/// <param name="counterparty">The name of the counterparty of the account.</param>
	/// <param name="inCurrencyId">The id of the account in currency.</param>
	public AccountOverviewRow(Guid id, string name, string currency, decimal balance, bool disabled, string counterparty, Guid inCurrencyId)
	{
		Id = id;
		Name = name;
		Currency = currency;
		Balance = balance;
		Disabled = disabled;
		Counterparty = counterparty;
		InCurrencyId = inCurrencyId;
	}

	/// <summary>Gets the id of the account.</summary>
	public Guid Id { get; }

	/// <summary>Gets the name of the account.</summary>
	public string Name { get; }

	/// <summary>Gets the currency of the account.</summary>
	public string Currency { get; }

	/// <summary>Gets or sets the balance of the account.</summary>
	public decimal Balance
	{
		get => _balance;
		set => SetAndNotify(ref _balance, value);
	}

	/// <summary>Gets a value indicating whether this account is disabled.</summary>
	public bool Disabled { get; }

	/// <summary>Gets the name of the counterparty of the account.</summary>
	public string Counterparty { get; }

	internal Guid InCurrencyId { get; }
}
