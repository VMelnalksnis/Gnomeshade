// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gnomeshade.WebApi.V1.OpenApi;

internal static class SwaggerGenOptionsExtensions
{
	internal static void SwaggerDocV1_0(this SwaggerGenOptions options)
	{
		options.SwaggerDoc(
			"v1.0",
			new()
			{
				Title = "Gnomeshade",
				Version = "v1.0",
				Description = "Personal finance tracking API",
				License = new()
				{
					Name = "AGPL-3.0-or-later",
					Url = new("https://www.gnu.org/licenses/agpl-3.0.txt"),
				},
			});
	}
}
