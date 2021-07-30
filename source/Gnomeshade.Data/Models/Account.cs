// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Gnomeshade.Data.Models.Abstractions;
using Gnomeshade.Data.Repositories.Extensions;

namespace Gnomeshade.Data.Models
{
	/// <summary>
	/// Represents an account which can hold funds one or more currencies.
	/// </summary>
	public sealed record Account :
		IOwnableEntity,
		IModifiableEntity,
		INamedEntity,
		IDisableableEntity
	{
		/// <inheritdoc />
		public Guid Id { get; init; }

		/// <inheritdoc />
		public DateTimeOffset CreatedAt { get; init; }

		/// <inheritdoc />
		public Guid OwnerId { get; set; }

		/// <inheritdoc />
		public Guid CreatedByUserId { get; init; }

		/// <inheritdoc />
		public DateTimeOffset ModifiedAt { get; set; }

		/// <inheritdoc />
		public Guid ModifiedByUserId { get; set; }

		/// <inheritdoc />
		public string Name { get; set; } = null!;

		/// <inheritdoc />
		public string NormalizedName { get; set; } = null!;

		/// <summary>
		/// Gets or sets the id of the preferred <see cref="AccountInCurrency"/>.
		/// </summary>
		public Guid PreferredCurrencyId { get; set; }

		/// <summary>
		/// Gets or sets the preferred currency.
		/// </summary>
		public Currency PreferredCurrency { get; set; } = null!;

		/// <inheritdoc />
		public DateTimeOffset? DisabledAt { get; set; }

		/// <inheritdoc />
		public Guid? DisabledByUserId { get; set; }

		/// <summary>
		/// Gets or sets the Business Identifier Code (BIC).
		/// </summary>
		public string? Bic { get; set; }

		/// <summary>
		/// Gets or sets the International Bank Account Number (IBAN).
		/// </summary>
		public string? Iban { get; set; }

		/// <summary>
		/// Gets or sets the account number, which does not follow standards such as IBAN.
		/// </summary>
		public string? AccountNumber { get; set; }

		/// <summary>
		/// Gets or sets the currencies used in this account.
		/// </summary>
		public List<AccountInCurrency> Currencies { get; set; } = null!;

		/// <summary>
		/// Initializes an account from a grouping of currencies.
		/// </summary>
		/// <param name="grouping">A grouping of currencies by account.</param>
		/// <returns>An account with initialized <see cref="Currencies"/>.</returns>
		public static Account FromGrouping(IGrouping<Account, OneToOne<Account, AccountInCurrency>> grouping)
		{
			grouping.Key.Currencies = grouping.Select(oneToOne => oneToOne.Second).ToList();
			return grouping.Key;
		}
	}
}
