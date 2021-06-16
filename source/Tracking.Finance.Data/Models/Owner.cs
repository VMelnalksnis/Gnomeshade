// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Models
{
	/// <summary>
	/// Represents a collection of other entities (users, roles, groups, etc.) that can own other entities.
	/// </summary>
	/// <seealso cref="Ownership"/>
	/// <seealso cref="User"/>
	public sealed record Owner : IEntity
	{
		/// <inheritdoc/>
		public Guid Id { get; init; }

		/// <inheritdoc/>
		public DateTimeOffset CreatedAt { get; init; }
	}
}
