// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Accounts
{
	[PublicAPI]
	public sealed record AccountModel
	{
		public Guid Id { get; init; }

		public DateTimeOffset CreatedAt { get; init; }

		public Guid OwnerId { get; init; }

		public Guid CreatedByUserId { get; init; }

		public DateTimeOffset ModifiedAt { get; init; }

		public Guid ModifiedByUserId { get; init; }

		public string Name { get; init; }

		public CurrencyModel PreferredCurrency { get; init; }

		public string? Bic { get; init; }

		public string? Iban { get; init; }

		public string? AccountNumber { get; init; }

		public List<AccountInCurrencyModel> Currencies { get; init; }
	}
}
