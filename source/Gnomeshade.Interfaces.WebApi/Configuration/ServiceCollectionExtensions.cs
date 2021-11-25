// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gnomeshade.Interfaces.WebApi.Configuration
{
	/// <summary>
	/// Provides methods to register configuration options in a <see cref="IServiceCollection"/>.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Registers the specified <typeparamref name="TOptions"/> in a service collection with data annotation validation.
		/// </summary>
		/// <typeparam name="TOptions">The options type to add.</typeparam>
		/// <param name="services">The service collection to which to add the options.</param>
		/// <param name="configuration">The configuration to which to bind the options.</param>
		/// <returns><paramref name="services"/> for chaining calls.</returns>
		/// <seealso cref="ConfigurationExtensions.GetSectionName(System.Type)"/>
		public static IServiceCollection AddValidatedOptions<TOptions>(
			this IServiceCollection services,
			IConfiguration configuration)
			where TOptions : class
		{
			var sectionName = typeof(TOptions).GetSectionName();
			services
				.AddOptions<TOptions>()
				.Bind(configuration.GetSection(sectionName))
				.ValidateDataAnnotations();

			return services;
		}

		public static TOptions AddAndGetOptions<TOptions>(
			this IServiceCollection services,
			IConfiguration configuration)
			where TOptions : class
		{
			services.AddValidatedOptions<TOptions>(configuration);
			return configuration.GetValid<TOptions>();
		}
	}
}
