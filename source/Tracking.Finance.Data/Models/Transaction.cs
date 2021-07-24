// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Tracking.Finance.Data.Models.Abstractions;
using Tracking.Finance.Data.Repositories.Extensions;

namespace Tracking.Finance.Data.Models
{
	public sealed record Transaction : IOwnableEntity, IModifiableEntity
	{
		/// <inheritdoc/>
		public Guid Id { get; init; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; init; }

		/// <inheritdoc/>
		public Guid CreatedByUserId { get; init; }

		/// <inheritdoc/>
		public Guid OwnerId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		/// <inheritdoc/>
		public Guid ModifiedByUserId { get; set; }

		public DateTimeOffset Date { get; set; }

		public string? Description { get; set; }

		public bool Generated { get; set; }

		public bool Validated { get; set; }

		public bool Completed { get; set; }

		public List<TransactionItem> Items { get; set; } = null!;

		/// <summary>
		/// Gets or sets a hash value of the import source information.
		/// </summary>
		public byte[]? ImportHash { get; set; }

		public static Transaction FromGrouping(IGrouping<Transaction, OneToOne<Transaction, TransactionItem>> grouping)
		{
			grouping.Key.Items = grouping.Select(oneToOne => oneToOne.Second).ToList();
			return grouping.Key;
		}
	}
}
