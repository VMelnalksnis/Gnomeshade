// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.Interfaces.WebApi.Models.Transactions;

/// <summary>A pending transfer between to a counterparty.</summary>
/// <seealso cref="PendingTransferCreation"/>
[PublicAPI]
public sealed record PendingTransfer
{
	/// <summary>The id of the transfer.</summary>
	public Guid Id { get; init; }

	/// <summary>The point in time when the transfer was created.</summary>
	public Instant CreatedAt { get; init; }

	/// <summary>The id of the owner of the transfer.</summary>
	public Guid OwnerId { get; init; }

	/// <summary>The id of the user that created this transfer.</summary>
	public Guid CreatedByUserId { get; init; }

	/// <summary>The point in the when the transfer was last modified.</summary>
	public Instant ModifiedAt { get; init; }

	/// <summary>The id of the user that last modified this transfer.</summary>
	public Guid ModifiedByUserId { get; init; }

	/// <summary>The id of transaction this transfer is a part of.</summary>
	/// <seealso cref="Transaction"/>
	public Guid TransactionId { get; init; }

	/// <summary>The amount withdrawn from the source account.</summary>
	public decimal SourceAmount { get; init; }

	/// <summary>The id of the account from which currency is withdrawn from.</summary>
	/// <seealso cref="AccountInCurrency"/>
	public Guid SourceAccountId { get; init; }

	/// <summary>The id of the counterparty to which currency will be deposited to.</summary>
	/// <seealso cref="Counterparty"/>
	public Guid TargetCounterpartyId { get; init; }

	/// <summary>The id of the transfer representing the completed transfer.</summary>
	/// <seealso cref="Transfer"/>
	public Guid? TransferId { get; init; }
}
