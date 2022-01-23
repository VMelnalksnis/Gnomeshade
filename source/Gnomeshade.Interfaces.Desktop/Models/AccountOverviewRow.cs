// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.Desktop.ViewModels.Binding;

namespace Gnomeshade.Interfaces.Desktop.Models;

/// <summary>
/// Single row in account overview.
/// </summary>
public sealed class AccountOverviewRow : PropertyChangedBase
{
	private readonly string _name = string.Empty;
	private readonly string _currency = string.Empty;
	private readonly bool _disabled;

	/// <summary>
	/// Initializes a new instance of the <see cref="AccountOverviewRow"/> class.
	/// </summary>
	/// <param name="id">The id of the account.</param>
	public AccountOverviewRow(Guid id)
	{
		Id = id;
	}

	/// <summary>
	/// Gets the id of the account.
	/// </summary>
	public Guid Id { get; }

	/// <summary>
	/// Gets the name of the account.
	/// </summary>
	public string Name
	{
		get => _name;
		init => SetAndNotify(ref _name, value, nameof(Name));
	}

	/// <summary>
	/// Gets the currency of the account.
	/// </summary>
	public string Currency
	{
		get => _currency;
		init => SetAndNotify(ref _currency, value, nameof(Currency));
	}

	/// <summary>
	/// Gets a value indicating whether this account is disabled.
	/// </summary>
	public bool Disabled
	{
		get => _disabled;
		init => SetAndNotify(ref _disabled, value, nameof(Disabled));
	}
}
