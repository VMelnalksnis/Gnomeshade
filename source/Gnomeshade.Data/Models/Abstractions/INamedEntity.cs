// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Gnomeshade.Data.Models.Abstractions
{
	/// <summary>
	/// Represents an entity that can be named by the user.
	/// </summary>
	public interface INamedEntity : IEntity
	{
		/// <summary>
		/// Gets or sets the name of the entity.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Gets or sets the normalized name of the entity.
		/// </summary>
		string NormalizedName { get; set; }
	}
}
