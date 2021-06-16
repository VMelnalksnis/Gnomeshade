// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;

using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tracking.Finance.Interfaces.WebApi.OpenApi
{
	/// <summary>
	/// Swagger filter that removes version parameter from operations and inserts the correct version in paths.
	/// </summary>
	public class ApiVersioningFilter : IOperationFilter, IDocumentFilter
	{
		/// <inheritdoc/>
		void IDocumentFilter.Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
		{
			var paths = new OpenApiPaths();
			foreach (var (key, pathItem) in swaggerDoc.Paths)
			{
				paths.Add(key.Replace("v{version}", swaggerDoc.Info.Version), pathItem);
			}

			swaggerDoc.Paths = paths;
		}

		/// <inheritdoc/>
		void IOperationFilter.Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			var versionParameter = operation.Parameters.SingleOrDefault(parameter => parameter.Name == "version");
			if (versionParameter is null)
			{
				return;
			}

			operation.Parameters.Remove(versionParameter);
		}
	}
}
