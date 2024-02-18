// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Models.Accounts;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Loans;

/// <summary>A transfer of money between two counterparties with an agreement to pay it back.</summary>
/// <seealso cref="LoanCreation"/>
[PublicAPI]
public sealed record Loan
{
	/// <summary>The id of the loan.</summary>
	public Guid Id { get; set; }

	/// <summary>The point in time when the loan was created.</summary>
	public Instant CreatedAt { get; set; }

	/// <summary>The id of the user that created this loan.</summary>
	public Guid CreatedByUserId { get; set; }

	/// <summary>The id of the owner of the loan.</summary>
	public Guid OwnerId { get; set; }

	/// <summary>The point in the when the loan was last modified.</summary>
	public Instant ModifiedAt { get; set; }

	/// <summary>The id of the user that last modified this loan.</summary>
	public Guid ModifiedByUserId { get; set; }

	/// <summary>The name of the loan.</summary>
	public string Name { get; set; } = null!;

	/// <summary>The id of the counterparty that issued this loan.</summary>
	/// <seealso cref="Counterparty"/>
	public Guid IssuingCounterpartyId { get; set; }

	/// <summary>The id of the counterparty that received this loan.</summary>
	/// <seealso cref="Counterparty"/>
	public Guid ReceivingCounterpartyId { get; set; }

	/// <summary>The amount of capital originally borrowed or invested.</summary>
	public decimal Principal { get; set; }

	/// <summary>The id of the currency of the <see cref="Principal"/>.</summary>
	/// <seealso cref="Currency"/>
	public Guid CurrencyId { get; set; }
}
