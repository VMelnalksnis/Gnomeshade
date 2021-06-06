// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

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
