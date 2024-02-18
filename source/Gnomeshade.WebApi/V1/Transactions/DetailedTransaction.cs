// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using Gnomeshade.WebApi.Models;
using Gnomeshade.WebApi.Models.Transactions;

using JetBrains.Annotations;

using NodaTime;

#pragma warning disable SA1623

namespace Gnomeshade.WebApi.V1.Transactions;

/// <summary>A <see cref="Transaction"/> with all sub-resources and additional details.</summary>
[PublicAPI]
[Obsolete]
public sealed record DetailedTransaction : Transaction
{
	/// <summary>The point in time when this transaction was posted to an account on the account servicer accounting books.</summary>
	public Instant? BookedAt { get; set; }

	/// <summary>The point in time when assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public Instant? ValuedAt { get; set; }

	/// <summary>All transfers in the transaction.</summary>
	public List<Transfer> Transfers { get; set; } = null!;

	/// <summary>The total balance of the transfers for the user.</summary>
	public decimal TransferBalance { get; set; }

	/// <summary>All the purchases in the transaction.</summary>
	public List<Purchase> Purchases { get; set; } = null!;

	/// <summary>The sum of all the prices from <see cref="Purchases"/>.</summary>
	public decimal PurchaseTotal { get; set; }

	/// <summary>All the loans in the transaction.</summary>
	public List<Loan> Loans { get; set; } = null!;

	/// <summary>The sum of all the amounts from <see cref="Loans"/>.</summary>
	public decimal LoanTotal { get; set; }

	/// <summary>All the links attached to the transaction.</summary>
	public List<Link> Links { get; set; } = null!;

	/// <summary>Creates a new instance of <see cref="DetailedTransaction"/> from a <see cref="Transaction"/>.</summary>
	/// <param name="transaction">The transaction from which to copy values.</param>
	/// <returns>A detailed transaction with all the properties from the transaction.</returns>
	public static DetailedTransaction FromTransaction(Transaction transaction) => new()
	{
		Id = transaction.Id,
		OwnerId = transaction.OwnerId,
		CreatedAt = transaction.CreatedAt,
		CreatedByUserId = transaction.CreatedByUserId,
		ModifiedAt = transaction.ModifiedAt,
		ModifiedByUserId = transaction.ModifiedByUserId,
		Description = transaction.Description,
		ImportedAt = transaction.ImportedAt,
		ReconciledAt = transaction.ReconciledAt,
	};
}
