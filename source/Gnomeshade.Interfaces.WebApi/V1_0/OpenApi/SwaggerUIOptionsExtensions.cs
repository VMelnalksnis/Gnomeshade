// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.AspNetCore.Builder;

using Swashbuckle.AspNetCore.SwaggerUI;

namespace Gnomeshade.Interfaces.WebApi.V1_0.OpenApi;

internal static class SwaggerUIOptionsExtensions
{
	internal static void SwaggerEndpointV1_0(this SwaggerUIOptions options)
	{
		options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Finance Tracker API v1.0");
	}
}
