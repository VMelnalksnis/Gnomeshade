using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tracking.Finance.Interfaces.WebApi.OpenApi
{
	public sealed class ValidationProblemDetailsFilter : ISchemaFilter, IOperationFilter
	{
		/// <inheritdoc/>
		void ISchemaFilter.Apply(OpenApiSchema swaggerDoc, SchemaFilterContext context)
		{
			context.SchemaGenerator.GenerateSchema(typeof(ValidationProblemDetails), context.SchemaRepository);
		}

		/// <inheritdoc/>
		void IOperationFilter.Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			var apiControllerAttributes =
				context
					.MethodInfo
					.DeclaringType?
					.GetCustomAttributes(true)
					.OfType<ApiControllerAttribute>();

			if (apiControllerAttributes is null || !apiControllerAttributes.Any())
			{
				return;
			}

			if (!operation.Parameters.Any() && !(operation.RequestBody?.Required ?? false))
			{
				return;
			}

			operation.Responses.Add(
				"400",
				new OpenApiResponse
				{
					Content = new Dictionary<string, OpenApiMediaType>
					{
						{
							"application/problem+json", new OpenApiMediaType
							{
								Schema = context.SchemaRepository.Schemas[nameof(ValidationProblemDetails)],
							}
						},
					},
					Description = "Bad request",
				});
		}
	}
}
