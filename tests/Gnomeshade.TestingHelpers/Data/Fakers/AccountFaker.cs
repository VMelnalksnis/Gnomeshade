// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.TestingHelpers.Data.Fakers;

/// <summary>
/// Generates fake <see cref="AccountEntity"/> objects.
/// </summary>
public sealed class AccountFaker : NamedEntityFaker<AccountEntity>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AccountFaker"/> class with the specified relationships.
	/// </summary>
	/// <param name="user">The user which created the account.</param>
	/// <param name="counterparty">The counterparty to which this account belongs to.</param>
	/// <param name="currency">The preferred currency.</param>
	public AccountFaker(UserEntity user, CounterpartyEntity counterparty, CurrencyEntity currency)
		: this(user.Id, counterparty.Id, currency.Id)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AccountFaker"/> class with the specified relationship ids.
	/// </summary>
	/// <param name="userId">The id of the <see cref="UserEntity"/> which created the account.</param>
	/// <param name="counterpartyId">The id of the <see cref="CounterpartyEntity"/> to which this account belongs to.</param>
	/// <param name="currencyId">The id of the preferred <see cref="CurrencyEntity"/>.</param>
	public AccountFaker(Guid userId, Guid counterpartyId, Guid currencyId)
		: base(userId)
	{
		RuleFor(account => account.CounterpartyId, counterpartyId);
		RuleFor(account => account.PreferredCurrencyId, currencyId);
		RuleFor(account => account.Name, faker => faker.Finance.AccountName());
		RuleFor(account => account.NormalizedName, (_, account) => account.Name.ToUpperInvariant());
		RuleFor(account => account.Bic, faker => faker.Finance.Bic());
		RuleFor(account => account.Iban, faker => faker.Finance.Iban());
		RuleFor(account => account.AccountNumber, faker => faker.Finance.Account());
		RuleFor(account => account.Currencies, () => new()
		{
			new()
			{
				OwnerId = userId,
				CreatedByUserId = userId,
				ModifiedByUserId = userId,
				CurrencyId = currencyId,
			},
		});
	}
}
