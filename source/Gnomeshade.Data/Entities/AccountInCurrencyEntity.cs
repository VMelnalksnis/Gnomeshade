// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities.Abstractions;

using NodaTime;

namespace Gnomeshade.Data.Entities;

/// <summary>Represents a single currency of an <see cref="AccountEntity"/>.</summary>
public sealed record AccountInCurrencyEntity : Entity, IOwnableEntity, IModifiableEntity
{
	/// <inheritdoc />
	public Guid OwnerId { get; set; }

	/// <inheritdoc />
	public Instant ModifiedAt { get; set; }

	/// <inheritdoc />
	public Guid ModifiedByUserId { get; set; }

	/// <summary>Gets or sets the id of the <see cref="AccountEntity"/> which holds this currency.</summary>
	public Guid AccountId { get; set; }

	/// <summary>Gets or sets the id of the <see cref="Currency"/> this account represents.</summary>
	public Guid CurrencyId { get; set; }

	/// <summary>Gets or sets the currency this account represents.</summary>
	public CurrencyEntity Currency { get; set; } = null!;
}
