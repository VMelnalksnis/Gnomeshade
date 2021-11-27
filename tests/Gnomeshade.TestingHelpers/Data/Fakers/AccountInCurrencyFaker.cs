// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.TestingHelpers.Data.Fakers;

/// <summary>
/// Generates fake <see cref="AccountInCurrencyEntity"/> objects.
/// </summary>
public sealed class AccountInCurrencyFaker : ModifiableEntityFaker<AccountInCurrencyEntity>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AccountInCurrencyFaker"/> class with the specified relationships.
	/// </summary>
	/// <param name="user">The user which created this account.</param>
	/// <param name="account">The account to which the currency is added to.</param>
	/// <param name="currency">The currency added to the account.</param>
	public AccountInCurrencyFaker(UserEntity user, AccountEntity account, CurrencyEntity currency)
		: this(user.Id, account.Id, currency.Id)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AccountInCurrencyFaker"/> class with the specified relationship ids.
	/// </summary>
	/// <param name="userId">The id of the <see cref="UserEntity"/> which created this account.</param>
	/// <param name="accountId">The id of the <see cref="AccountEntity"/> to which the currency is added to.</param>
	/// <param name="currencyId">The id of the <see cref="CurrencyEntity"/> added to the account.</param>
	public AccountInCurrencyFaker(Guid userId, Guid accountId, Guid currencyId)
		: base(userId)
	{
		RuleFor(account => account.AccountId, accountId);
		RuleFor(account => account.CurrencyId, currencyId);
	}
}
