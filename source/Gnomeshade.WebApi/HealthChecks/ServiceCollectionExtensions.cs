// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;

using Gnomeshade.WebApi.Configuration.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using VMelnalksnis.NordigenDotNet;

namespace Gnomeshade.WebApi.HealthChecks;

internal static class ServiceCollectionExtensions
{
	internal static IServiceCollection AddGnomeshadeHealthChecks(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var builder = services.AddHealthChecks();

		services.AddTransient<DatabaseHealthCheck>();
		builder.AddCheck<DatabaseHealthCheck>("database");

		if (configuration.GetChildren().Any(section => section.Key is NordigenOptions.SectionName))
		{
			services.AddTransient<NordigenHealthCheck>();
			builder.AddCheck<NordigenHealthCheck>("nordigen");
		}

		if (configuration.GetChildren().Any(section => section.Key is OidcProviderOptions.OidcProviderSectionName))
		{
			services.AddTransient<OidcHealthCheck>();
			builder.AddCheck<OidcHealthCheck>("oidc");
		}

		return services;
	}
}
