// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Gnomeshade.Data.Entities.Abstractions;
using Gnomeshade.Data.Repositories.Extensions;

namespace Gnomeshade.Data.Entities
{
	/// <summary>
	/// A single financial transaction.
	/// </summary>
	public sealed record TransactionEntity : IOwnableEntity, IModifiableEntity
	{
		/// <inheritdoc/>
		public Guid Id { get; init; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; init; }

		/// <inheritdoc/>
		public Guid OwnerId { get; set; }

		/// <inheritdoc/>
		public Guid CreatedByUserId { get; init; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		/// <inheritdoc/>
		public Guid ModifiedByUserId { get; set; }

		/// <summary>
		/// Gets or sets the point in time when this transaction was completed.
		/// </summary>
		public DateTimeOffset Date { get; set; }

		/// <summary>
		/// Gets or sets the description of this transaction.
		/// </summary>
		public string? Description { get; set; }

		/// <summary>
		/// Gets or sets the point in time when this transaction was imported.
		/// </summary>
		public DateTimeOffset? ImportedAt { get; set; }

		/// <summary>
		/// Gets or sets a hash value of the import source information.
		/// </summary>
		public byte[]? ImportHash { get; set; }

		/// <summary>
		/// Gets or sets the point in time when this transaction was validated.
		/// </summary>
		public DateTimeOffset? ValidatedAt { get; set; }

		/// <summary>
		/// Gets or sets the id of the user that validated the transaction.
		/// </summary>
		public Guid? ValidatedByUserId { get; set; }

		/// <summary>
		/// Gets or sets the transaction items of this transaction.
		/// </summary>
		public List<TransactionItemEntity> Items { get; set; } = null!;

		/// <summary>
		/// Initializes a transaction from a grouping of transaction items.
		/// </summary>
		/// <param name="grouping">A grouping of transaction items by transaction.</param>
		/// <returns>A transaction with initialized <see cref="Items"/>.</returns>
		public static TransactionEntity FromGrouping(IGrouping<Guid, OneToOne<TransactionEntity, TransactionItemEntity>> grouping)
		{
			var transaction = grouping.First().First;
			transaction.Items = grouping.Select(oneToOne => oneToOne.Second).ToList();
			return transaction;
		}
	}
}
