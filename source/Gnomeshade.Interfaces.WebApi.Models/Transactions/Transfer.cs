// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Transactions;

/// <summary>A transfer between two accounts.</summary>
/// <seealso cref="TransferCreation"/>
[PublicAPI]
public sealed record Transfer
{
	/// <summary>The id of the transfer.</summary>
	public Guid Id { get; init; }

	/// <summary>The point in time when the transfer was created.</summary>
	public DateTimeOffset CreatedAt { get; init; }

	/// <summary>The id of the owner of the transfer.</summary>
	public Guid OwnerId { get; init; }

	/// <summary>The id of the user that created this transfer.</summary>
	public Guid CreatedByUserId { get; init; }

	/// <summary>The point in the when the transfer was last modified.</summary>
	public DateTimeOffset ModifiedAt { get; init; }

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

	/// <summary>The amount deposited in the target account.</summary>
	public decimal TargetAmount { get; init; }

	/// <summary>The id of the account to which currency is deposited to.</summary>
	/// <seealso cref="AccountInCurrency"/>
	public Guid TargetAccountId { get; init; }

	/// <summary>The reference id issued by the bank.</summary>
	public string? BankReference { get; init; }

	/// <summary>The reference id issued by an external source.</summary>
	public string? ExternalReference { get; init; }

	/// <summary>The reference id issued by the user.</summary>
	public string? InternalReference { get; init; }
}
