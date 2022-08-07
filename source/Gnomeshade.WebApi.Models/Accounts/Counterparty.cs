// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

using NodaTime;

namespace Gnomeshade.WebApi.Models.Accounts;

/// <summary>A party that participates in a financial transaction.</summary>
[PublicAPI]
public sealed record Counterparty
{
	/// <summary>The id of the counterparty.</summary>
	public Guid Id { get; init; }

	/// <summary>The point in time when this counterparty was created. </summary>
	public Instant CreatedAt { get; init; }

	/// <summary>The id of the owner of this counterparty.</summary>
	public Guid OwnerId { get; init; }

	/// <summary>The id of the user which created this counterparty.</summary>
	public Guid CreatedByUserId { get; init; }

	/// <summary>The point in time when this counterparty was last modified.</summary>
	public Instant ModifiedAt { get; init; }

	/// <summary>The id of the user which last modified this counterparty.</summary>
	public Guid ModifiedByUserId { get; init; }

	/// <summary>The name of the counterparty.</summary>
	public string Name { get; init; } = null!;
}
