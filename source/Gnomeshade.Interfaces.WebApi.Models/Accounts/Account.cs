// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

#pragma warning disable SA1623
namespace Gnomeshade.Interfaces.WebApi.Models.Accounts
{
	[PublicAPI]
	public sealed record Account
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
		/// The id of the counterparty to which this account belongs to.
		/// </summary>
		public Guid CounterpartyId { get; init; }

		/// <summary>
		/// The preferred currency of the account.
		/// </summary>
		public CurrencyModel PreferredCurrency { get; init; } = null!;

		/// <summary>
		/// The point in time when this account was disabled.
		/// </summary>
		public DateTimeOffset? DisabledAt { get; init; }

		/// <summary>
		/// The id of the user which disabled this account.
		/// </summary>
		public Guid? DisabledByUserId { get; init; }

		/// <summary>
		/// Whether or not this account is disabled.
		/// </summary>
		public bool Disabled => DisabledAt.HasValue;

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
		public List<AccountInCurrency> Currencies { get; init; } = null!;
	}
}
