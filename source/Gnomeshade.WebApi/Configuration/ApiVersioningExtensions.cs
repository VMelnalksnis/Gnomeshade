// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Asp.Versioning;
using Asp.Versioning.Conventions;

using Gnomeshade.WebApi.Configuration.Swagger;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gnomeshade.WebApi.Configuration;

internal static class ApiVersioningExtensions
{
	internal static IServiceCollection AddGnomeshadeApiVersioning(this IServiceCollection services)
	{
		services
			.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerGenConfigureOptions>()
			.AddApiVersioning(options =>
			{
				options.DefaultApiVersion = new(1, 0);
				options.AssumeDefaultVersionWhenUnspecified = true;
				options.ReportApiVersions = true;
				options.ApiVersionReader = new UrlSegmentApiVersionReader();
			})
			.AddMvc(options => options.Conventions.Add(new VersionByNamespaceConvention()))
			.AddApiExplorer(options =>
			{
				options.GroupNameFormat = "'v'VVV";
				options.SubstituteApiVersionInUrl = true;
			});

		return services;
	}
}
