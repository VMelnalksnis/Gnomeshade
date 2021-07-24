// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Transactions
{
	/// <summary>
	/// Represents information needed in order to create a transaction.
	/// </summary>
	[PublicAPI]
	public sealed record TransactionCreationModel
	{
		/// <summary>
		/// Gets the date on which the transaction was completed on.
		/// </summary>
		[Required]
		public DateTimeOffset? Date { get; init; }

		public string? Description { get; init; }

		[DefaultValue(true)]
		public bool Generated { get; init; } = true;

		[DefaultValue(false)]
		public bool Validated { get; init; }

		[DefaultValue(false)]
		public bool Completed { get; init; }

		[MinLength(64)]
		[MaxLength(64)]
		public byte[]? ImportHash { get; init; }

		public List<TransactionItemCreationModel>? Items { get; init; }
	}
}
