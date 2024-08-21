// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Asp.Versioning.ApiExplorer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gnomeshade.WebApi.Configuration.Swagger;

internal sealed class SwaggerGenConfigureOptions : IConfigureOptions<SwaggerGenOptions>
{
	private readonly IApiVersionDescriptionProvider _versionDescriptionProvider;

	public SwaggerGenConfigureOptions(IApiVersionDescriptionProvider versionDescriptionProvider)
	{
		_versionDescriptionProvider = versionDescriptionProvider;
	}

	/// <inheritdoc/>
	public void Configure(SwaggerGenOptions options)
	{
		foreach (var versionDescription in _versionDescriptionProvider.ApiVersionDescriptions)
		{
			var title = "Gnomeshade API";
			if (versionDescription.IsDeprecated)
			{
				title = $"{title} (Deprecated)";
			}

			options.SwaggerDoc(
				versionDescription.GroupName,
				new()
				{
					Title = title,
					Description = "Personal finance tracking API",
					Version = versionDescription.ApiVersion.ToString(),
					License = new()
					{
						Name = "AGPL-3.0-or-later",
						Url = new("https://www.gnu.org/licenses/agpl-3.0.txt"),
					},
				});
		}
	}
}
