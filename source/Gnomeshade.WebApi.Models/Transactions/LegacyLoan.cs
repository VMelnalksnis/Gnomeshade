// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Models.Accounts;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>An amount that was loaned or payed back as a part of a transaction.</summary>
[PublicAPI]
[Obsolete]
public sealed record LegacyLoan
{
	/// <summary>The id of the loan.</summary>
	public Guid Id { get; set; }

	/// <summary>The id of the owner of the loan.</summary>
	public Guid OwnerId { get; set; }

	/// <summary>The point in time when the loan was created.</summary>
	public Instant CreatedAt { get; set; }

	/// <summary>The id of the user that created this loan.</summary>
	public Guid CreatedByUserId { get; set; }

	/// <summary>The point in the when the loan was last modified.</summary>
	public Instant ModifiedAt { get; set; }

	/// <summary>The id of the user that last modified this loan.</summary>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>The id of the the transaction this loan is a part of.</summary>
	/// <seealso cref="Transaction"/>
	public Guid TransactionId { get; set; }

	/// <summary>The id of the counterparty the gave (issued) the loan to <see cref="ReceivingCounterpartyId"/>.</summary>
	/// <seealso cref="Counterparty"/>
	public Guid IssuingCounterpartyId { get; set; }

	/// <summary>The id of the counterparty the received the loan from <see cref="IssuingCounterpartyId"/>.</summary>
	/// <seealso cref="Counterparty"/>
	public Guid ReceivingCounterpartyId { get; set; }

	/// <summary>The amount that was loaned or payed back.</summary>
	public decimal Amount { get; set; }

	/// <summary>The id of the currency of the <see cref="Amount"/>.</summary>
	/// <seealso cref="Currency"/>
	public Guid CurrencyId { get; set; }
}
