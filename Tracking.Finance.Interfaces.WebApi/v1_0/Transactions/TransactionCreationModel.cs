// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Transactions
{
	/// <summary>
	/// Represents information needed in order to create a transaction.
	/// </summary>
	public record TransactionCreationModel
	{
		/// <summary>
		/// The date on which the transaction was completed on.
		/// </summary>
		[Required]
		public DateTimeOffset? Date { get; init; }

		public string? Description { get; init; }

		[DefaultValue(true)]
		public bool Generated { get; init; } = true;

		[DefaultValue(false)]
		public bool Validated { get; init; } = false;

		[DefaultValue(false)]
		public bool Completed { get; init; } = false;

		public List<TransactionItemCreationModel>? Items { get; init; }
	}
}
