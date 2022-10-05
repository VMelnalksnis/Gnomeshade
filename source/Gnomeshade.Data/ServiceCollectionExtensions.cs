// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Data.Repositories;

using Microsoft.Extensions.DependencyInjection;

namespace Gnomeshade.Data;

/// <summary>Extension methods for configuration.</summary>
public static class ServiceCollectionExtensions
{
	/// <summary>Adds all data services.</summary>
	/// <param name="services">The service collection in which to register the services.</param>
	/// <returns><paramref name="services"/>.</returns>
	public static IServiceCollection AddRepositories(this IServiceCollection services)
	{
		return services
			.AddScoped<OwnerRepository>()
			.AddScoped<OwnershipRepository>()
			.AddScoped<TransactionRepository>()
			.AddScoped<PurchaseRepository>()
			.AddScoped<TransferRepository>()
			.AddScoped<LoanRepository>()
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
			.AddScoped<TransactionUnitOfWork>()
			.AddScoped<UserUnitOfWork>();
	}
}
