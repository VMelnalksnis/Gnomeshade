// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.Models.Owners;

/// <summary>Access rights for a user to a group of resources.</summary>
[PublicAPI]
public sealed record Ownership
{
	/// <summary>The id of the ownership.</summary>
	public Guid Id { get; init; }

	/// <summary>The id of the owner.</summary>
	public Guid OwnerId { get; init; }

	/// <summary>The id of the user that has the access.</summary>
	public Guid UserId { get; init; }

	/// <summary>The id of the access level.</summary>
	public Guid AccessId { get; init; }
}
