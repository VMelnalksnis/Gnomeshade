// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Models
{
	public sealed class ProductCategoryClosure : IEntity
	{
		/// <inheritdoc/>
		public int Id { get; set; }

		public int ParentProductCategoryId { get; set; }

		public int ChildProductCategoryId { get; set; }

		public byte Depth { get; set; }
	}
}
