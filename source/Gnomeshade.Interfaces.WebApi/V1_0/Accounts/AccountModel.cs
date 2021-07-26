// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Accounts
{
	[PublicAPI]
	[SuppressMessage("ReSharper", "SA1623", Justification = "Documentation for public API.")]
	public sealed record AccountModel
	{
		/// <summary>
		/// The id of the account.
		/// </summary>
		public Guid Id { get; init; }

		/// <summary>
		/// The point in time when this account was created.
		/// </summary>
		public DateTimeOffset CreatedAt { get; init; }

		/// <summary>
		/// The id of the owner of this account.
		/// </summary>
		public Guid OwnerId { get; init; } // todo is this relevant?

		/// <summary>
		/// The id of the user which created this account.
		/// </summary>
		public Guid CreatedByUserId { get; init; }

		/// <summary>
		/// The point in time when this account was last modified.
		/// </summary>
		public DateTimeOffset ModifiedAt { get; init; }

		/// <summary>
		/// The id of the user which last modified this account.
		/// </summary>
		public Guid ModifiedByUserId { get; init; }

		/// <summary>
		/// The name of the account.
		/// </summary>
		public string Name { get; init; } = null!;

		/// <summary>
		/// The preferred currency of the account.
		/// </summary>
		public CurrencyModel PreferredCurrency { get; init; } = null!;

		/// <summary>
		/// The BIC (Business Identifier Code) of the account.
		/// </summary>
		public string? Bic { get; init; }

		/// <summary>
		/// The IBAN (International Bank Account Number) of the account.
		/// </summary>
		public string? Iban { get; init; }

		/// <summary>
		/// The account number of the account.
		/// </summary>
		public string? AccountNumber { get; init; }

		/// <summary>
		/// A collection of currencies available for the account.
		/// </summary>
		public List<AccountInCurrencyModel> Currencies { get; init; } = null!;
	}
}
