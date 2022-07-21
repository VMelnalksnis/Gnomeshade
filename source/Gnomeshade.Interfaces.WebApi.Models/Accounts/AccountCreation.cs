// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Accounts;

/// <summary>The information needed to create a new account.</summary>
[PublicAPI]
public sealed record AccountCreation : Creation
{
	/// <inheritdoc cref="Account.Name"/>
	[Required]
	public string? Name { get; init; }

	/// <inheritdoc cref="Account.CounterpartyId"/>
	[Required]
	public Guid? CounterpartyId { get; init; }

	/// <summary>The id of the preferred currency of the account.</summary>
	[Required]
	public Guid? PreferredCurrencyId { get; init; }

	/// <inheritdoc cref="Account.Bic"/>
	public string? Bic { get; init; }

	/// <inheritdoc cref="Account.Iban"/>
	public string? Iban { get; init; }

	/// <inheritdoc cref="Account.AccountNumber"/>
	public string? AccountNumber { get; init; }

	/// <inheritdoc cref="Account.Currencies"/>
	[Required]
	[MinLength(1)]
	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = $"Implements {nameof(ICollection)}")]
	public List<AccountInCurrencyCreation>? Currencies { get; init; }
}
