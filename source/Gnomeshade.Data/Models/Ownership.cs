// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Models.Abstractions;

namespace Gnomeshade.Data.Models
{
	/// <summary>
	/// Link between <see cref="Owner"/> and the entities which represent the user,
	/// for example <see cref="User"/>.
	/// </summary>
	public sealed class Ownership : IEntity
	{
		/// <inheritdoc/>
		public Guid Id { get; init; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; init; }

		/// <summary>
		/// Gets or sets the id of the <see cref="Owner"/> to which the other entities are linked.
		/// </summary>
		public Guid OwnerId { get; set; }

		/// <summary>
		/// Gets or sets the id of the <see cref="User"/> which is linked to the <see cref="Owner"/> with id <see cref="OwnerId"/>.
		/// </summary>
		public Guid UserId { get; set; }
	}
}
