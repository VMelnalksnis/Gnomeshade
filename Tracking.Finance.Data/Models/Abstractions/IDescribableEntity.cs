﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Tracking.Finance.Data.Models.Abstractions
{
	public interface IDescribableEntity
	{
		/// <summary>
		/// Gets the description of this entity.
		/// </summary>
		string? Description { get; }
	}
}
