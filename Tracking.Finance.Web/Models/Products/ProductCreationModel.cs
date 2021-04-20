using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tracking.Finance.Web.Models.Products
{
	public class ProductCreationModel
	{
		[Required]
		public int? FinanceUserId { get; set; }

		[Required]
		public int? ProductCategoryId { get; set; }

		[Required]
		public int? UnitId { get; set; }

		[Required]
		public int? SupplierId { get; set; }

		[Required]
		[DataType(DataType.Text)]
		public string? Name { get; set; }

		[DataType(DataType.MultilineText)]
		public string? Description { get; set; }

		public IEnumerable<SelectListItem> Categories { get; set; }

		public IEnumerable<SelectListItem> Units { get; set; }

		public IEnumerable<SelectListItem> Suppliers { get; set; }
	}
}
