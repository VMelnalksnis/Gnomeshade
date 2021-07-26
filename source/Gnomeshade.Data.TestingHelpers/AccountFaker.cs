// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Models;

namespace Gnomeshade.Data.TestingHelpers
{
	/// <summary>
	/// Generates fake <see cref="Account"/> objects.
	/// </summary>
	public sealed class AccountFaker : ModifiableEntityFaker<Account>
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

		/// <summary>
		/// Generates an account unique from <paramref name="account"/>.
		/// </summary>
		/// <param name="account">An account against which to compare for uniqueness.</param>
		/// <param name="attemptCount">The number of times to try to generate a unique account.</param>
		/// <returns>A fake <see cref="Account"/> unique from <paramref name="account"/>.</returns>
		/// <exception cref="InvalidOperationException">Failed to generate a unique account after <paramref name="attemptCount"/> attempts.</exception>
		public Account GenerateUnique(Account account, int attemptCount = 10)
		{
			for (var i = 0; i < attemptCount; i++)
			{
				var generatedAccount = Generate();
				if (account.NormalizedName != generatedAccount.NormalizedName)
				{
					return generatedAccount;
				}
			}

			throw new InvalidOperationException($"Failed to generate unique account after {attemptCount} attempts");
		}
	}
}
