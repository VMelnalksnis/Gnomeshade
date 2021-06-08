﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Tracking.Finance.Data.Models.Abstractions
{
	/// <summary>
	/// Represents an entity that can be modified.
	/// </summary>
	/// <seealso cref="User"/>
	public interface IModifiableEntity : IEntity
	{
		/// <summary>
		/// Gets or sets the timestamp of the last modification of this entity.
		/// </summary>
		DateTimeOffset ModifiedAt { get; set; }

		/// <summary>
		/// Gets or sets the id of the <see cref="User"/> which last modified this entity.
		/// </summary>
		public Guid ModifiedByUserId { get; set; }
	}
}
