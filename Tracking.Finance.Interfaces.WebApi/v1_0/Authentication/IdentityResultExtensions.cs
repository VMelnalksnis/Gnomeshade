// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Authentication
{
	public static class IdentityResultExtensions
	{
		public static ValidationProblemDetails GetProblemDetails(
			this IdentityResult identityResult,
			ProblemDetailsFactory problemDetailsFactory,
			HttpContext httpContext)
		{
			var errors = new ModelStateDictionary();
			foreach (var error in identityResult.Errors)
			{
				errors.AddModelError(error.Code, error.Description);
			}

			return problemDetailsFactory.CreateValidationProblemDetails(httpContext, errors);
		}
	}
}
