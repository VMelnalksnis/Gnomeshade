// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models.Owners;

/// <summary>The level of access a user can have to a group of resources.</summary>
[PublicAPI]
public sealed record Access
{
	/// <summary>The id of the access.</summary>
	public Guid Id { get; init; }

	/// <summary>The name of the access.</summary>
	public string Name { get; init; } = null!;
}
