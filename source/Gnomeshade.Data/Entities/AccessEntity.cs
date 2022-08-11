// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Data.Entities.Abstractions;

namespace Gnomeshade.Data.Entities;

/// <summary>Represents an access level to another entity.</summary>
public sealed record AccessEntity : Entity, INamedEntity
{
	/// <inheritdoc />
	public string Name { get; set; } = null!;

	/// <inheritdoc />
	public string NormalizedName { get; set; } = null!;
}
