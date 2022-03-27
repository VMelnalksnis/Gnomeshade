﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Purchases;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Transfers;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions;

/// <summary>Overview of a single <see cref="Transaction"/>.</summary>
public sealed class TransactionOverview : PropertyChangedBase
{
	/// <summary>Initializes a new instance of the <see cref="TransactionOverview"/> class.</summary>
	/// <param name="id">The id of the transactions.</param>
	/// <param name="bookedAt">The point in time when this transaction was posted to an account on the account servicer accounting books.</param>
	/// <param name="valuedAt">The point in time when assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</param>
	/// <param name="description">The description of the transaction.</param>
	/// <param name="importedAt">The point in time when this transaction was imported.</param>
	/// <param name="reconciledAt">The point in time when this transaction was reconciled.</param>
	/// <param name="transfers">All transfers of the transaction.</param>
	/// <param name="purchases">All purchases of the transaction.</param>
	public TransactionOverview(
		Guid id,
		DateTimeOffset? bookedAt,
		DateTimeOffset? valuedAt,
		string? description,
		DateTimeOffset? importedAt,
		DateTimeOffset? reconciledAt,
		List<TransferOverview> transfers,
		List<PurchaseOverview> purchases)
	{
		Id = id;
		BookedAt = bookedAt;
		ValuedAt = valuedAt;
		Description = description;
		ImportedAt = importedAt;
		ReconciledAt = reconciledAt;
		Transfers = transfers;
		Purchases = purchases;
	}

	/// <summary>Gets the id of the transactions.</summary>
	public Guid Id { get; }

	/// <summary>Gets the point in time when this transaction was posted to an account on the account servicer accounting books.</summary>
	public DateTimeOffset? BookedAt { get; }

	/// <summary>Gets the point in time when assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public DateTimeOffset? ValuedAt { get; }

	/// <summary>Gets the description of the transaction.</summary>
	public string? Description { get; }

	/// <summary>Gets the point in time when this transaction was imported.</summary>
	public DateTimeOffset? ImportedAt { get; }

	/// <summary>Gets the point in time when this transaction was reconciled.</summary>
	public DateTimeOffset? ReconciledAt { get; }

	/// <summary>Gets all transfers of the transaction.</summary>
	public List<TransferOverview> Transfers { get; }

	/// <summary>Gets all purchases of the transaction.</summary>
	public List<PurchaseOverview> Purchases { get; }
}
