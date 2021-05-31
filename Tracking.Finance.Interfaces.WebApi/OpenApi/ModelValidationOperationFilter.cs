using System.Collections.Generic;
using System.Linq;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using Tracking.Finance.Interfaces.WebApi.Validation;

namespace Tracking.Finance.Interfaces.WebApi.OpenApi
{
	public sealed class ModelValidationOperationFilter : IOperationFilter
	{
		/// <inheritdoc/>
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			if (context.MethodInfo.GetCustomAttributes(true).OfType<ModelValidationAttribute>().Any())
			{
				operation.Responses.Add("400", new OpenApiResponse
				{
					Description = "Validation failed",
					Content = new Dictionary<string, OpenApiMediaType>
					{
						{
							"application/json", new OpenApiMediaType
							{
								Example = new OpenApiObject
								{
									{ "type", new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1") },
									{ "title", new OpenApiString("One or more validation errors occurred.") },
									{ "status", new OpenApiInteger(400) },
									{ "traceId", new OpenApiString("00-074e0100aa3f86408098f4f78e5e2923-5cb80c7a4a681c47-00") },
									{
										"errors",
										new OpenApiObject
										{
											{ "Field", new OpenApiArray { new OpenApiString("Error details for Field.") } },
											{ "OtherField", new OpenApiArray { new OpenApiString("Error details for OtherField.") } },
										}
									},
								},
							}
						},
					},
				});
			}
		}
	}
}
