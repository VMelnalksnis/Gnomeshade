// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gnomeshade.Interfaces.Avalonia.Core.Configuration;

/// <summary>Methods for configuring dependency injection.</summary>
public static class ServiceCollectionExtensions
{
	/// <summary>Bind all required configuration options for Gnomeshade.</summary>
	/// <param name="serviceCollection">The service collection in which to register the options.</param>
	/// <param name="configuration">The configuration to which to bind to.</param>
	/// <returns><paramref name="serviceCollection"/> for chaining service registration calls.</returns>
	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Unused configuration can be trimmed")]
	public static IServiceCollection AddGnomeshadeOptions(
		this IServiceCollection serviceCollection,
		IConfiguration configuration)
	{
		serviceCollection
			.AddOptions<UserConfiguration>()
			.Bind(configuration);

		serviceCollection
			.AddOptions<OidcOptions>()
			.Bind(configuration.GetSection(nameof(UserConfiguration.Oidc)))
			.ValidateDataAnnotations();

		return serviceCollection;
	}
}
