﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Accounts;

/// <summary>An account in one or multiple currencies, which belongs to a counterparty.</summary>
[PublicAPI]
public sealed record Account
{
	/// <summary>The id of the account.</summary>
	public Guid Id { get; set; }

	/// <summary>The point in time when this account was created.</summary>
	public Instant CreatedAt { get; set; }

	/// <summary>The id of the owner of this account.</summary>
	public Guid OwnerId { get; set; }

	/// <summary>The id of the user which created this account.</summary>
	public Guid CreatedByUserId { get; set; }

	/// <summary>The point in time when this account was last modified.</summary>
	public Instant ModifiedAt { get; set; }

	/// <summary>The id of the user which last modified this account.</summary>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>The name of the account.</summary>
	public string Name { get; set; } = null!;

	/// <summary>The id of the counterparty to which this account belongs to.</summary>
	public Guid CounterpartyId { get; set; }

	/// <summary>The id of the preferred currency of the account.</summary>
	public Guid PreferredCurrencyId { get; set; }

	/// <summary>The BIC (Business Identifier Code) of the account.</summary>
	public string? Bic { get; set; }

	/// <summary>The IBAN (International Bank Account Number) of the account.</summary>
	public string? Iban { get; set; }

	/// <summary>The account number of the account.</summary>
	public string? AccountNumber { get; set; }

	/// <summary>A collection of currencies available for the account.</summary>
	public List<AccountInCurrency> Currencies { get; set; } = null!;
}
