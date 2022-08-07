// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Gnomeshade.WebApi.HealthChecks;

internal static class ServiceCollectionExtensions
{
	internal static IServiceCollection AddGnomeshadeHealthChecks(this IServiceCollection services)
	{
		services
			.AddTransient<DatabaseHealthCheck>()
			.AddTransient<NordigenHealthCheck>()
			.AddTransient<OidcHealthCheck>();

		services
			.AddHealthChecks()
			.AddCheck<DatabaseHealthCheck>("database")
			.AddCheck<NordigenHealthCheck>("nordigen")
			.AddCheck<OidcHealthCheck>("oidc");

		return services;
	}
}
