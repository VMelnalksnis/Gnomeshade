// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Accounts
{
	[PublicAPI]
	public sealed record AccountCreationModel
	{
		[Required]
		public string? Name { get; init; }

		[Required]
		public Guid? PreferredCurrencyId { get; init; }

		public string? Bic { get; init; }

		public string? Iban { get; init; }

		public string? AccountNumber { get; init; }

		[Required]
		[MinLength(1)]
		public List<AccountInCurrencyCreationModel>? Currencies { get; init; }
	}
}
