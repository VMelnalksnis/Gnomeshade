// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

using Gnomeshade.Data.Identity;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

	internal static IdentityBuilder AddIdentity(this IServiceCollection services)
	{
		services.AddHttpContextAccessor();

		// Identity services
		services.TryAddScoped<IUserValidator<ApplicationUser>, UserValidator<ApplicationUser>>();
		services.TryAddScoped<IPasswordValidator<ApplicationUser>, PasswordValidator<ApplicationUser>>();
		services.TryAddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
		services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
		services.TryAddScoped<IRoleValidator<ApplicationRole>, RoleValidator<ApplicationRole>>();

		// No interface for the error describer so we can add errors without rev'ing the interface
		services.TryAddScoped<IdentityErrorDescriber>();
		services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<ApplicationUser>>();
		services.TryAddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<ApplicationUser>>();
		services.TryAddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>>();
		services.TryAddScoped<IUserConfirmation<ApplicationUser>, DefaultUserConfirmation<ApplicationUser>>();
		services.TryAddScoped<UserManager<ApplicationUser>>();
		services.TryAddScoped<SignInManager<ApplicationUser>>();
		services.TryAddScoped<RoleManager<ApplicationRole>>();

		return new(typeof(ApplicationUser), typeof(ApplicationRole), services);
	}
}
