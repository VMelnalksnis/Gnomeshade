using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tracking.Finance.Web.Models.Units
{
	public class UnitCreationModel
	{
		[Required]
		public int? FinanceUserId { get; set; }

		[Required]
		public short? Exponent { get; set; }

		[Required]
		public decimal? Mantissa { get; set; }

		[Required]
		[DataType(DataType.Text)]
		public string? Name { get; set; }

		public int? ParentUnitId { get; set; }

		public IEnumerable<SelectListItem> Units { get; set; }
	}
}
