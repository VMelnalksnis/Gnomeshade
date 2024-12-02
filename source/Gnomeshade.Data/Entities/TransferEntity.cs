// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>Represents a transfer between two accounts.</summary>
public sealed record TransferEntity : TransferBase
{
	/// <summary>Gets or sets the id of the account from which currency is withdrawn from.</summary>
	/// <seealso cref="AccountInCurrencyEntity"/>
	public Guid SourceAccountId { get; set; }

	/// <summary>Gets or sets the id of the <see cref="AccountInCurrencyEntity"/> to which currency is deposited to.</summary>
	/// <seealso cref="AccountInCurrencyEntity"/>
	public Guid TargetAccountId { get; set; }

	/// <summary>Gets or sets a reference id issued by the bank.</summary>
	public string? BankReference { get; set; }

	/// <summary>Gets or sets a reference id issued by an external source.</summary>
	public string? ExternalReference { get; set; }

	/// <summary>Gets or sets a reference id issued by the user.</summary>
	public string? InternalReference { get; set; }

	/// <summary>Gets or sets the point in time when assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public Instant? ValuedAt { get; set; }
}
