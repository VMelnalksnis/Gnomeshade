// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Repositories;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gnomeshade.Data;

/// <summary>Extension methods for configuration.</summary>
public static class ServiceCollectionExtensions
{
	/// <summary>Adds all data services.</summary>
	/// <param name="services">The service collection in which to register the services.</param>
	/// <returns><paramref name="services"/>.</returns>
	public static IServiceCollection AddRepositories(this IServiceCollection services)
	{
#pragma warning disable CS0612 // Type or member is obsolete
		services.AddScoped<LoanRepository>();
#pragma warning restore CS0612 // Type or member is obsolete

		return services
			.AddScoped<OwnerRepository>()
			.AddScoped<OwnershipRepository>()
			.AddScoped<TransactionRepository>()
			.AddScoped<PurchaseRepository>()
			.AddScoped<TransferRepository>()
			.AddScoped<Loan2Repository>()
			.AddScoped<LoanPaymentRepository>()
			.AddScoped<UserRepository>()
			.AddScoped<AccountRepository>()
			.AddScoped<AccountInCurrencyRepository>()
			.AddScoped<CurrencyRepository>()
			.AddScoped<ProductRepository>()
			.AddScoped<UnitRepository>()
			.AddScoped<CounterpartyRepository>()
			.AddScoped<LinkRepository>()
			.AddScoped<AccessRepository>()
			.AddScoped<AccountUnitOfWork>()
			.AddScoped<CategoryRepository>()
			.AddScoped<ProjectRepository>()
			.AddScoped<TransactionUnitOfWork>()
			.AddScoped<UserUnitOfWork>();
	}

	/// <summary>Adds an Entity Framework implementation of identity information stores.</summary>
	/// <param name="identityBuilder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
	/// <returns><paramref name="identityBuilder"/> with identity stored added.</returns>
	public static IdentityBuilder AddIdentityStores(this IdentityBuilder identityBuilder)
	{
		var services = identityBuilder.Services;
		services.TryAddScoped<IUserStore<ApplicationUser>, ApplicationUserStore>();
		services.TryAddScoped<IRoleStore<ApplicationRole>, ApplicationRoleStore>();

		return identityBuilder;
	}
}
