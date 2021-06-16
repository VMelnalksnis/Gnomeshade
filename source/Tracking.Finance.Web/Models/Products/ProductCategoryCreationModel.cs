using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tracking.Finance.Web.Models.Products
{
	public class ProductCategoryCreationModel
	{
		[Required]
		public int? FinanceUserId { get; set; }

		[Required]
		[DataType(DataType.Text)]
		public string? Name { get; set; }

		public int? ParentCategoryId { get; set; }

		public IEnumerable<SelectListItem> Categories { get; set; }
	}
}
