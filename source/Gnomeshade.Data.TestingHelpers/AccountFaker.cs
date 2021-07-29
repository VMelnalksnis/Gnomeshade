﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Models;

namespace Gnomeshade.Data.TestingHelpers
{
	/// <summary>
	/// Generates fake <see cref="Account"/> objects.
	/// </summary>
	public sealed class AccountFaker : NamedEntityFaker<Account>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AccountFaker"/> class with the specified relationships.
		/// </summary>
		/// <param name="user">The user which created the account.</param>
		/// <param name="currency">The preferred currency.</param>
		public AccountFaker(User user, Currency currency)
			: this(user.Id, currency.Id)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AccountFaker"/> class with the specified relationship ids.
		/// </summary>
		/// <param name="userId">The id of the <see cref="User"/> which created the account.</param>
		/// <param name="currencyId">The id of the preferred <see cref="Currency"/>.</param>
		public AccountFaker(Guid userId, Guid currencyId)
			: base(userId)
		{
			RuleFor(account => account.PreferredCurrencyId, currencyId);
			RuleFor(account => account.Name, faker => faker.Finance.AccountName());
			RuleFor(account => account.NormalizedName, (_, account) => account.Name.ToUpperInvariant());
			RuleFor(account => account.Bic, faker => faker.Finance.Bic());
			RuleFor(account => account.Iban, faker => faker.Finance.Iban());
			RuleFor(account => account.AccountNumber, faker => faker.Finance.Account());
		}
	}
}
