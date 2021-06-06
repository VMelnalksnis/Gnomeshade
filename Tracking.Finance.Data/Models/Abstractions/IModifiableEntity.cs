// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Tracking.Finance.Data.Models.Abstractions
{
	public interface IModifiableEntity
	{
		DateTimeOffset CreatedAt { get; set; }

		public int CreatedByUserId { get; set; }

		DateTimeOffset ModifiedAt { get; set; }

		public int ModifiedByUserId { get; set; }
	}
}
