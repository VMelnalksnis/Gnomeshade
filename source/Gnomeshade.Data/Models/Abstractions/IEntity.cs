// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Data.Models.Abstractions
{
	/// <summary>
	/// Represents an entity.
	/// </summary>
	public interface IEntity
	{
		/// <summary>
		/// Gets the unique id of the entity.
		/// </summary>
		public Guid Id { get; init; }

		/// <summary>
		/// Gets the timestamp of the creation of this entity.
		/// </summary>
		public DateTimeOffset CreatedAt { get; init; }
	}
}
