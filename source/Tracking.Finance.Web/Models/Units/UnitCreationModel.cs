using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Units
{
	/// <summary>
	/// Information needed to create a <see cref="Unit"/>.
	/// </summary>
	public class UnitCreationModel
	{
		/// <summary>
		/// Gets the id of the <see cref="FinanceUser"/> to which the <see cref="Unit"/> belongs to.
		/// </summary>
		[Required]
		public int? FinanceUserId { get; init; }

		/// <summary>
		/// Gets or sets the exponent of the scaling multiplier from <see cref="ParentUnitId"/>.
		/// </summary>
		[Required]
		public short Exponent { get; set; } = 1;

		/// <summary>
		/// Gets or sets the mantissa of the scaling multiplier from <see cref="ParentUnitId"/>.
		/// </summary>
		[Required]
		public decimal Mantissa { get; set; } = 1;

		/// <summary>
		/// Gets or sets the name of the <see cref="Unit"/>.
		/// </summary>
		[Required]
		[DataType(DataType.Text)]
		public string? Name { get; set; }

		/// <summary>
		/// Gets or sets the id of the parent <see cref="Unit"/>.
		/// </summary>
		public int? ParentUnitId { get; set; }

		/// <summary>
		/// Gets or sets a collection of <see cref="SelectListItem"/> containing units that can be set as <see cref="ParentUnitId"/>.
		/// </summary>
		public List<SelectListItem> Units { get; init; } = new List<SelectListItem>();
	}
}
