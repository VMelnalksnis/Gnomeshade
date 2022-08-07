// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Transactions;

/// <summary>A <see cref="Transaction"/> with all sub-resources and additional details.</summary>
[PublicAPI]
public sealed record DetailedTransaction : Transaction
{
	/// <summary>All transfers in the transaction.</summary>
	public List<Transfer> Transfers { get; init; } = null!;

	/// <summary>The total balance of the transfers for the user.</summary>
	public decimal TransferBalance { get; init; }

	/// <summary>All the purchases in the transaction.</summary>
	public List<Purchase> Purchases { get; init; } = null!;

	/// <summary>The sum of all the prices from <see cref="Purchases"/>.</summary>
	public decimal PurchaseTotal { get; init; }

	/// <summary>All the loans in the transaction.</summary>
	public List<Loan> Loans { get; init; } = null!;

	/// <summary>The sum of all the amounts from <see cref="Loans"/>.</summary>
	public decimal LoanTotal { get; init; }

	/// <summary>All the links attached to the transaction.</summary>
	public List<Link> Links { get; init; } = null!;

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
		BookedAt = transaction.BookedAt,
		ValuedAt = transaction.ValuedAt,
		Description = transaction.Description,
		ImportedAt = transaction.ImportedAt,
		ReconciledAt = transaction.ReconciledAt,
	};
}
