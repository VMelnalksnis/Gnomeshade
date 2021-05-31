using Microsoft.AspNetCore.Builder;

using Swashbuckle.AspNetCore.SwaggerUI;

namespace Tracking.Finance.Interfaces.WebApi.v1_0.OpenApi
{
	public static class SwaggerUIOptionsExtensions
	{
		/// <summary>
		/// Adds Swagger JSON endpoint for version 1.0.
		/// </summary>
		///
		/// <param name="options">The <see cref="SwaggerUIOptions"/> to which to add the endpoint.</param>
		public static void SwaggerEndpointV1_0(this SwaggerUIOptions options)
		{
			options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Finance Tracker API v1.0");
		}
	}
}
