// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Gnomeshade.Data.Repositories;

/// <summary>Defines the level of access needed for the particular entity.</summary>
public enum AccessLevel
{
	/// <summary>Allows read-only access.</summary>
	Read,

	/// <summary>Allows to update an existing entity.</summary>
	Write,

	/// <summary>Allows to delete an existing entity.</summary>
	Delete,
}
