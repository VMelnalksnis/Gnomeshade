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
public sealed record Transfer : TransferBase
{
	/// <summary>The id of the account from which currency is withdrawn from.</summary>
	/// <seealso cref="AccountInCurrency"/>
	public Guid SourceAccountId { get; set; }

	/// <summary>The id of the account to which currency is deposited to.</summary>
	/// <seealso cref="AccountInCurrency"/>
	public Guid TargetAccountId { get; set; }

	/// <summary>The reference id issued by the bank.</summary>
	public string? BankReference { get; set; }

	/// <summary>The reference id issued by an external source.</summary>
	public string? ExternalReference { get; set; }

	/// <summary>The reference id issued by the user.</summary>
	public string? InternalReference { get; set; }

	/// <summary>The point in time when assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public Instant? ValuedAt { get; set; }
}
