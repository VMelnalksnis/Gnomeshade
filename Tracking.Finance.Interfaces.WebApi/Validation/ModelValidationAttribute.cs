using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Tracking.Finance.Interfaces.WebApi.Validation
{
	public sealed class ModelValidationAttribute : ActionFilterAttribute
	{
		/// <inheritdoc/>
		public sealed override void OnActionExecuting(ActionExecutingContext context)
		{
			if (!context.ModelState.IsValid)
			{
				context.Result = new BadRequestObjectResult(context.ModelState);
			}
		}
	}
}
