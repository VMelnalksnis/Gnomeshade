// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using static System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes;

using static JetBrains.Annotations.ImplicitUseKindFlags;
using static JetBrains.Annotations.ImplicitUseTargetFlags;

namespace Gnomeshade.WebApi.Configuration;

internal static class ServiceCollectionExtensions
{
	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = $"{nameof(DynamicallyAccessedMembersAttribute)} indicates what is dynamically accessed")]
	internal static IServiceCollection AddValidatedOptions<[DynamicallyAccessedMembers(All), MeansImplicitUse(Assign, Members)] TOptions>(
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
