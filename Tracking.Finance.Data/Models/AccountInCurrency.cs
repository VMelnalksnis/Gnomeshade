﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Models
{
	public sealed class AccountInCurrency : IEntity, IUserSpecificEntity, IModifiableEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		/// <inheritdoc/>
		public int UserId { get; set; }

		public int AccountId { get; set; }

		public int CurrencyId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; set; }

		/// <inheritdoc/>
		public int CreatedByUserId { get; set; }

		/// <inheritdoc/>
		public DateTimeOffset ModifiedAt { get; set; }

		/// <inheritdoc/>
		public int ModifiedByUserId { get; set; }
	}
}
