using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tracking.Finance.Interfaces.WebApi.OpenApi
{
	public sealed class ValidationProblemDetailsSchemaFilter : SchemaFilter<ValidationProblemDetails>
	{
		/// <inheritdoc/>
		protected sealed override void ApplyFilter(OpenApiSchema schema, SchemaFilterContext context)
		{
			schema.Properties["status"].Default = new OpenApiInteger(400);
		}
	}
}
