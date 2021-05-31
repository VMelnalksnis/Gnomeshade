using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tracking.Finance.Interfaces.WebApi.OpenApi
{
	/// <summary>
	/// Swagger documentation document filter that inserts the actual API version parameters in operations.
	/// </summary>
	public class ReplaceVersionWithExactValueInPathFilter : IDocumentFilter
	{
		/// <inheritdoc/>
		public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
		{
			var paths = new OpenApiPaths();
			foreach (var path in swaggerDoc.Paths)
			{
				paths.Add(path.Key.Replace("v{version}", swaggerDoc.Info.Version), path.Value);
			}

			swaggerDoc.Paths = paths;
		}
	}
}
