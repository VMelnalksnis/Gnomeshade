// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gnomeshade.WebApi.OpenApi;

/// <summary>An <see cref="ISchemaFilter"/> applied only to a specific type.</summary>
/// <typeparam name="T">The <see cref="SchemaFilterContext.Type"/> to which to apply the filer.</typeparam>
public abstract class SchemaFilter<T> : ISchemaFilter
{
	/// <inheritdoc/>
	public void Apply(OpenApiSchema schema, SchemaFilterContext context)
	{
		if (typeof(T) != context.Type)
		{
			return;
		}

		ApplyFilter(schema);
	}

	/// <inheritdoc cref="Apply"/>
	protected abstract void ApplyFilter(OpenApiSchema schema);
}
