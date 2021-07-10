// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Accounts
{
	public sealed record AccountInCurrencyModel
	{
		public Guid Id { get; init; }

		public DateTimeOffset CreatedAt { get; init; }

		public Guid OwnerId { get; init; }

		public Guid CreatedByUserId { get; init; }

		public DateTimeOffset ModifiedAt { get; init; }

		public Guid ModifiedByUserId { get; init; }

		public CurrencyModel Currency { get; init; }
	}
}
