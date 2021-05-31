using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tracking.Finance.Interfaces.WebApi.OpenApi
{
	/// <summary>
	/// Adds <see cref="StatusCodes.Status500InternalServerError"/> response to all operations.
	/// </summary>
	public sealed class InternalServerErrorOperationFilter : IOperationFilter
	{
		/// <inheritdoc/>
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			if (!operation.Responses.ContainsKey("500"))
			{
				operation.Responses.Add(
					"500",
					new OpenApiResponse
					{
						Description = "Internal Server Error",
					});
			}
		}
	}
}
