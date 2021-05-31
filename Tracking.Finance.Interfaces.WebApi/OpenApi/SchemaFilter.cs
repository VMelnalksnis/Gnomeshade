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