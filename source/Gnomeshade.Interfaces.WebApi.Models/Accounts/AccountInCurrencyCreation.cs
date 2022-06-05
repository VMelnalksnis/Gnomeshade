// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Accounts;

/// <summary>The information needed to add a currency to an account.</summary>
[PublicAPI]
public sealed record AccountInCurrencyCreation : Creation
{
	/// <summary>The id of the currency to add to an account.</summary>
	[Required]
	public Guid? CurrencyId { get; init; }
}
