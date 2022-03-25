// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gnomeshade.Interfaces.WebApi.Configuration;

internal static class ServiceCollectionExtensions
{
	internal static IServiceCollection AddValidatedOptions<TOptions>(
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
}
