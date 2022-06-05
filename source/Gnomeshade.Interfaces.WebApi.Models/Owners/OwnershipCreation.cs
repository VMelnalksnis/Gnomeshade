// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Owners;

/// <summary>Information needed to create an ownership.</summary>
[PublicAPI]
public sealed record OwnershipCreation : Creation
{
	/// <inheritdoc cref="Ownership.UserId"/>
	public Guid UserId { get; init; }

	/// <inheritdoc cref="Ownership.AccessId"/>
	public Guid AccessId { get; init; }
}
