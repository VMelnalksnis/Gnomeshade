using System.Linq;

using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tracking.Finance.Interfaces.WebApi.OpenApi
{
	/// <summary>
	/// Swagger documentation operation filter that removes {version} parameter.
	/// </summary>
	public class RemoveVersionParameterFilter : IOperationFilter
	{
		/// <inheritdoc/>
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
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
