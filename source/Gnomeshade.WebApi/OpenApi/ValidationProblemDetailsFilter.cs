// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gnomeshade.WebApi.OpenApi;

/// <summary>Adds 400 response with <see cref="ValidationProblemDetails"/> to all operations.</summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class ValidationProblemDetailsFilter : ISchemaFilter, IOperationFilter
{
	/// <inheritdoc/>
	void ISchemaFilter.Apply(IOpenApiSchema schema, SchemaFilterContext context)
	{
		if (context.Type == typeof(ValidationProblemDetails) ||
			context.SchemaRepository.Schemas.ContainsKey(nameof(ValidationProblemDetails)))
		{
			return;
		}

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
				.OfType<ApiControllerAttribute>()
				.ToArray();

		if (apiControllerAttributes is null or [] ||
			(operation.Parameters is null or [] && operation.RequestBody?.Required is not true))
		{
			return;
		}

		operation.Responses ??= new();
		operation.Responses.Add(
			"400",
			new OpenApiResponse
			{
				Content = new Dictionary<string, OpenApiMediaType>
				{
					{
						"application/problem+json", new()
						{
							Schema = context.SchemaRepository.Schemas[nameof(ValidationProblemDetails)],
						}
					},
				},
				Description = "Bad request",
			});
	}
}
