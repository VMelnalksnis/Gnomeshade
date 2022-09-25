// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Models.Accounts;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>A transfer between two accounts.</summary>
/// <seealso cref="TransferCreation"/>
[PublicAPI]
public sealed record Transfer
{
	/// <summary>The id of the transfer.</summary>
	public Guid Id { get; set; }

	/// <summary>The point in time when the transfer was created.</summary>
	public Instant CreatedAt { get; set; }

	/// <summary>The id of the owner of the transfer.</summary>
	public Guid OwnerId { get; set; }

	/// <summary>The id of the user that created this transfer.</summary>
	public Guid CreatedByUserId { get; set; }

	/// <summary>The point in the when the transfer was last modified.</summary>
	public Instant ModifiedAt { get; set; }

	/// <summary>The id of the user that last modified this transfer.</summary>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>The id of transaction this transfer is a part of.</summary>
	/// <seealso cref="Transaction"/>
	public Guid TransactionId { get; set; }

	/// <summary>The amount withdrawn from the source account.</summary>
	public decimal SourceAmount { get; set; }

	/// <summary>The id of the account from which currency is withdrawn from.</summary>
	/// <seealso cref="AccountInCurrency"/>
	public Guid SourceAccountId { get; set; }

	/// <summary>The amount deposited in the target account.</summary>
	public decimal TargetAmount { get; set; }

	/// <summary>The id of the account to which currency is deposited to.</summary>
	/// <seealso cref="AccountInCurrency"/>
	public Guid TargetAccountId { get; set; }

	/// <summary>The reference id issued by the bank.</summary>
	public string? BankReference { get; set; }

	/// <summary>The reference id issued by an external source.</summary>
	public string? ExternalReference { get; set; }

	/// <summary>The reference id issued by the user.</summary>
	public string? InternalReference { get; set; }
}
