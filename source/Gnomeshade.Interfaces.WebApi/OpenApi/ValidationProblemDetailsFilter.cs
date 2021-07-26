// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gnomeshade.Interfaces.WebApi.OpenApi
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

			if (apiControllerAttributes is null ||
				!apiControllerAttributes.Any() ||
				(!operation.Parameters.Any() && !(operation.RequestBody?.Required ?? false)))
			{
				return;
			}

			operation.Responses.Add(
				"400",
				new()
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
