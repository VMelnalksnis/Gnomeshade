// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tracking.Finance.Interfaces.WebApi.OpenApi
{
	public abstract class SchemaFilter<T> : ISchemaFilter
	{
		/// <inheritdoc/>
		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			if (typeof(T) != context.Type)
			{
				return;
			}

			ApplyFilter(schema, context);
		}

		protected abstract void ApplyFilter(OpenApiSchema schema, SchemaFilterContext context);
	}
}