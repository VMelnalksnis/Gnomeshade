// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

using NodaTime;

#pragma warning disable SA1623
namespace Gnomeshade.WebApi.Models.Accounts;

/// <summary>
/// A single currency for a specific account.
/// </summary>
[PublicAPI]
public sealed record AccountInCurrency
{
	/// <summary>
	/// The id of the account in currency.
	/// </summary>
	public Guid Id { get; init; }

	/// <summary>
	/// The point in time when this account in currency was created.
	/// </summary>
	public Instant CreatedAt { get; init; }

	/// <summary>
	/// The id of the owner of this account in currency.
	/// </summary>
	public Guid OwnerId { get; init; }

	/// <summary>
	/// The id of the user which created this account in currency.
	/// </summary>
	public Guid CreatedByUserId { get; init; }

	/// <summary>
	/// The point in time when this account in currency was last modified.
	/// </summary>
	public Instant ModifiedAt { get; init; }

	/// <summary>
	/// The id of the user which last modified this account in currency.
	/// </summary>
	public Guid ModifiedByUserId { get; init; }

	/// <summary>
	/// The currency of the account in currency.
	/// </summary>
	public Currency Currency { get; init; } = null!;

	/// <summary>
	/// The point in time when this account in currency was disabled.
	/// </summary>
	public Instant? DisabledAt { get; init; }

	/// <summary>
	/// The id of the user which disabled this account in currency.
	/// </summary>
	public Guid? DisabledByUserId { get; init; }

	/// <summary>
	/// Whether or not this account in currency is disabled.
	/// </summary>
	public bool Disabled => DisabledAt.HasValue;
}
