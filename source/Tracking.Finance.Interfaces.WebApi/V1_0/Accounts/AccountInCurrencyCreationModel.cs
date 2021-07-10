// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Accounts
{
	public sealed record AccountInCurrencyCreationModel
	{
		[Required]
		public Guid? CurrencyId { get; init; }
	}
}
