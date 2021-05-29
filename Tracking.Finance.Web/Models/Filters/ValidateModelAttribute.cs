using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Tracking.Finance.Web.Models.Filters
{
	public class ValidateModelAttribute : ActionFilterAttribute
	{
		/// <inheritdoc/>
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (!context.ModelState.IsValid)
			{
				context.Result = new BadRequestObjectResult(context.ModelState);
			}
		}
	}
}
