// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Tracking.Finance.Data.Models.Abstractions
{
	/// <summary>
	/// Represents an entity.
	/// </summary>
	public interface IEntity
	{
		/// <summary>
		/// Gets the database Id of the entity.
		/// </summary>
		int Id { get; }
	}
}
