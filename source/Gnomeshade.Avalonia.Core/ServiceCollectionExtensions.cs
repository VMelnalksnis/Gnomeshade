// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Avalonia.Core.Accesses;
using Gnomeshade.Avalonia.Core.Accounts;
using Gnomeshade.Avalonia.Core.Authentication;
using Gnomeshade.Avalonia.Core.Configuration;
using Gnomeshade.Avalonia.Core.Counterparties;
using Gnomeshade.Avalonia.Core.Help;
using Gnomeshade.Avalonia.Core.Imports;
using Gnomeshade.Avalonia.Core.Products;
using Gnomeshade.Avalonia.Core.Reports;
using Gnomeshade.Avalonia.Core.Transactions;
using Gnomeshade.Avalonia.Core.Transactions.Loans.Migration;

using Microsoft.Extensions.DependencyInjection;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Methods for configuring needed services in <see cref="IServiceCollection"/>.</summary>
public static class ServiceCollectionExtensions
{
	/// <summary>Registers all view models that can be created via dependency injection.</summary>
	/// <param name="serviceCollection">The service collection into which to register the view models.</param>
	/// <returns><paramref name="serviceCollection"/>.</returns>
	public static IServiceCollection AddViewModels(this IServiceCollection serviceCollection) => serviceCollection
		.AddSingleton<MainWindowViewModel>()
		.AddSingleton<ConfigurationWizardViewModel>()
		.AddSingleton<GnomeshadeConfigurationViewModel>()
		.AddSingleton<AuthenticationConfigurationViewModel>()
		.AddSingleton<UserConfigurationWriter>()
		.AddSingleton<LoginViewModel>()
		.AddSingleton<ApplicationSettingsViewModel>()
		.AddSingleton<CounterpartyMergeViewModel>()
		.AddSingleton<CategoryViewModel>()
		.AddSingleton<AccountViewModel>()
		.AddSingleton<CounterpartyViewModel>()
		.AddSingleton<ImportViewModel>()
		.AddSingleton<ProductViewModel>()
		.AddSingleton<UnitViewModel>()
		.AddSingleton<CategoryReportViewModel>()
		.AddSingleton<BalanceReportViewModel>()
		.AddSingleton<ProductReportViewModel>()
		.AddSingleton<PreferencesViewModel>()
		.AddSingleton<TransactionViewModel>()
		.AddSingleton<OwnerViewModel>()
		.AddSingleton<OwnerUpsertionViewModel>()
		.AddSingleton<AboutViewModel>()
		.AddSingleton<LicensesViewModel>()
		.AddSingleton<LoanMigrationViewModel>();
}
