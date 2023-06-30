// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

using Avalonia.Controls.Notifications;

using Gnomeshade.Avalonia.Core.Accesses;
using Gnomeshade.Avalonia.Core.Accounts;
using Gnomeshade.Avalonia.Core.Authentication;
using Gnomeshade.Avalonia.Core.Configuration;
using Gnomeshade.Avalonia.Core.Counterparties;
using Gnomeshade.Avalonia.Core.Imports;
using Gnomeshade.Avalonia.Core.Products;
using Gnomeshade.Avalonia.Core.Reports;
using Gnomeshade.Avalonia.Core.Transactions;
using Gnomeshade.Avalonia.Core.Transactions.Controls;
using Gnomeshade.Avalonia.Core.Transactions.Links;
using Gnomeshade.Avalonia.Core.Transactions.Loans;
using Gnomeshade.Avalonia.Core.Transactions.Purchases;
using Gnomeshade.Avalonia.Core.Transactions.Transfers;
using Gnomeshade.WebApi.Client;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.DesignTime;

/// <summary>Data needed only during design time.</summary>
[SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access", Justification = "Static member order is important due to initialization order")]
public static class DesignTimeData
{
	private static IClock Clock => SystemClock.Instance;

	private static IDateTimeZoneProvider DateTimeZoneProvider => DateTimeZoneProviders.Tzdb;

	private static IActivityService ActivityService => new ActivityService(
		new(() => new WindowNotificationManager(null)),
		NullLogger<ActivityService>.Instance);

	private static DesignTimeGnomeshadeClient GnomeshadeClient => new();

	private static IDialogService DialogService { get; } = new DesignTimeDialogService();

	private static IAuthenticationService AuthenticationService { get; } =
		new AuthenticationService(GnomeshadeClient, new(Clock), new DesignTimeCredentialStorage(), new NullOidcClient());

	/// <summary>Gets an instance of <see cref="MainWindowViewModel"/> for use during design time.</summary>
	public static MainWindowViewModel MainWindowViewModel { get; } =
		new(GetServiceProvider());

	/// <summary>Gets an instance of <see cref="AccountUpsertionViewModel"/> for use during design time.</summary>
	public static AccountUpsertionViewModel AccountUpsertionViewModel { get; } =
		InitializeViewModel(new AccountUpsertionViewModel(ActivityService, GnomeshadeClient, null));

	/// <summary>Gets an instance of <see cref="AccountViewModel"/> for use during design time.</summary>
	public static AccountViewModel AccountViewModel { get; } =
		InitializeViewModel<AccountViewModel, AccountOverviewRow, AccountUpsertionViewModel>(new(ActivityService, GnomeshadeClient));

	/// <summary>Gets an instance of <see cref="CounterpartyViewModel"/> for use during design time.</summary>
	public static CounterpartyViewModel CounterpartyViewModel { get; } =
		InitializeViewModel(new CounterpartyViewModel(ActivityService, GnomeshadeClient));

	/// <summary>Gets an instance of <see cref="CounterpartyMergeViewModel"/> for use during design time.</summary>
	public static CounterpartyMergeViewModel CounterpartyMergeViewModel { get; } =
		InitializeViewModel(new CounterpartyMergeViewModel(ActivityService, GnomeshadeClient));

	/// <summary>Gets an instance of <see cref="CounterpartyUpsertionViewModel"/> for use during design time.</summary>
	public static CounterpartyUpsertionViewModel CounterpartyUpsertionViewModel { get; } =
		InitializeViewModel(new CounterpartyUpsertionViewModel(ActivityService, GnomeshadeClient, Guid.Empty));

	/// <summary>Gets an instance of <see cref="ImportViewModel"/> for use during design time.</summary>
	public static ImportViewModel ImportViewModel { get; } = new(ActivityService, GnomeshadeClient, new DesignTimeOptionsMonitor<PreferencesOptions>());

	/// <summary>Gets an instance of <see cref="LoginViewModel"/> for use during design time.</summary>
	public static LoginViewModel LoginViewModel { get; } = new(ActivityService, AuthenticationService);

	/// <summary>Gets an instance of <see cref="ProductUpsertionViewModel"/> for use during design time.</summary>
	public static ProductUpsertionViewModel ProductUpsertionViewModel { get; } =
		InitializeViewModel(new ProductUpsertionViewModel(ActivityService, GnomeshadeClient, DateTimeZoneProvider, null));

	/// <summary>Gets an instance of <see cref="UnitUpsertionViewModel"/> for use during design time.</summary>
	public static UnitUpsertionViewModel UnitUpsertionViewModel { get; } =
		InitializeViewModel(new UnitUpsertionViewModel(ActivityService, GnomeshadeClient, null));

	/// <summary>Gets an instance of <see cref="CategoryUpsertionViewModel"/> for use during design time.</summary>
	public static CategoryUpsertionViewModel CategoryUpsertionViewModel { get; } =
		InitializeViewModel(new CategoryUpsertionViewModel(ActivityService, GnomeshadeClient, null));

	/// <summary>Gets an instance of <see cref="CategoryViewModel"/> for use during design time.</summary>
	public static CategoryViewModel CategoryViewModel { get; } =
		InitializeViewModel<CategoryViewModel, CategoryRow, CategoryUpsertionViewModel>(new(ActivityService, GnomeshadeClient));

	/// <summary>Gets an instance of <see cref="TransactionProperties"/> for use during design time.</summary>
	public static TransactionProperties TransactionProperties { get; } = new(ActivityService);

	/// <summary>Gets an instance of <see cref="ProductViewModel"/> for use during design time.</summary>
	public static ProductViewModel ProductViewModel { get; } =
		InitializeViewModel<ProductViewModel, ProductRow, ProductUpsertionViewModel>(new(ActivityService, GnomeshadeClient, DateTimeZoneProvider));

	/// <summary>Gets an instance of <see cref="UnitViewModel"/> for use during design time.</summary>
	public static UnitViewModel UnitViewModel { get; } =
		InitializeViewModel<UnitViewModel, UnitRow, UnitUpsertionViewModel>(new(ActivityService, GnomeshadeClient));

	/// <summary>Gets an instance of <see cref="TransferUpsertionViewModel"/> for use during design time.</summary>
	public static TransferUpsertionViewModel TransferUpsertionViewModel { get; } =
		InitializeViewModel(new TransferUpsertionViewModel(ActivityService, GnomeshadeClient, DialogService, DateTimeZoneProvider, Guid.Empty, null));

	/// <summary>Gets an instance of <see cref="TransferViewModel"/> for use during design time.</summary>
	public static TransferViewModel TransferViewModel { get; } =
		InitializeViewModel<TransferViewModel, TransferOverview, TransferUpsertionViewModel>(new(ActivityService, GnomeshadeClient, DialogService, DateTimeZoneProvider, Guid.Empty));

	/// <summary>Gets an instance of <see cref="PurchaseUpsertionViewModel"/> for use during design time.</summary>
	public static PurchaseUpsertionViewModel PurchaseUpsertionViewModel { get; } =
		InitializeViewModel(new PurchaseUpsertionViewModel(ActivityService, GnomeshadeClient, DialogService, DateTimeZoneProvider, Guid.Empty, null));

	/// <summary>Gets an instance of <see cref="PurchaseViewModel"/> for use during design time.</summary>
	public static PurchaseViewModel PurchaseViewModel { get; } =
		InitializeViewModel<PurchaseViewModel, PurchaseOverview, PurchaseUpsertionViewModel>(new(ActivityService, GnomeshadeClient, DialogService, DateTimeZoneProvider, Guid.Empty));

	/// <summary>Gets an instance of <see cref="TransactionViewModel"/> for use during design time.</summary>
	public static TransactionViewModel TransactionViewModel { get; } =
		InitializeViewModel<TransactionViewModel, TransactionOverview, TransactionUpsertionViewModel>(new(ActivityService, GnomeshadeClient, DialogService, Clock, DateTimeZoneProvider));

	/// <summary>Gets an instance of <see cref="TransactionFilter"/> for use during design time.</summary>
	public static TransactionFilter TransactionFilter { get; } = new(ActivityService, Clock, DateTimeZoneProvider);

	/// <summary>Gets an instance of <see cref="TransactionUpsertionViewModel"/> for use during design time.</summary>
	public static TransactionUpsertionViewModel TransactionUpsertionViewModel { get; } =
		InitializeViewModel(new TransactionUpsertionViewModel(ActivityService, GnomeshadeClient, DialogService, Clock, DateTimeZoneProvider, Guid.Empty));

	/// <summary>Gets an instance of <see cref="LinkUpsertionViewModel"/> for use during design time.</summary>
	public static LinkUpsertionViewModel LinkUpsertionViewModel { get; } =
		InitializeViewModel(new LinkUpsertionViewModel(ActivityService, GnomeshadeClient, Guid.Empty, null));

	/// <summary>Gets an instance of <see cref="LinkViewModel"/> for use during design time.</summary>
	public static LinkViewModel LinkViewModel { get; } =
		InitializeViewModel<LinkViewModel, LinkOverview, LinkUpsertionViewModel>(new(ActivityService, GnomeshadeClient, Guid.Empty));

	/// <summary>Gets an instance of <see cref="LoanUpsertionViewModel"/> for use during design time.</summary>
	public static LoanUpsertionViewModel LoanUpsertionViewModel { get; } =
		InitializeViewModel(new LoanUpsertionViewModel(ActivityService, GnomeshadeClient, Guid.Empty, null));

	/// <summary>Gets an instance of <see cref="LoanViewModel"/> for use during design time.</summary>
	public static LoanViewModel LoanViewModel { get; } =
		InitializeViewModel<LoanViewModel, LoanOverview, LoanUpsertionViewModel>(new(ActivityService, GnomeshadeClient, Guid.Empty));

	/// <summary>Gets an instance of <see cref="CategoryReportViewModel"/> for use during design time.</summary>
	public static CategoryReportViewModel CategoryReportViewModel { get; } =
		InitializeViewModel(new CategoryReportViewModel(ActivityService, GnomeshadeClient, Clock, DateTimeZoneProvider));

	/// <summary>Gets an instance of <see cref="ProductReportViewModel"/> for use during design time.</summary>
	public static ProductReportViewModel ProductReportViewModel { get; } =
		InitializeViewModel(new ProductReportViewModel(ActivityService, GnomeshadeClient, Clock, DateTimeZoneProvider));

	/// <summary>Gets an instance of <see cref="BalanceReportViewModel"/> for use during design time.</summary>
	public static BalanceReportViewModel BalanceReportViewModel { get; } =
		InitializeViewModel(new BalanceReportViewModel(ActivityService, GnomeshadeClient, Clock, DateTimeZoneProvider));

	/// <summary>Gets an instance of <see cref="TransactionSummary"/> for use during design time.</summary>
	public static TransactionSummary TransactionSummary { get; } = new(ActivityService);

	/// <summary>Gets an instance of <see cref="ProductFilter"/> for use during design time.</summary>
	public static ProductFilter ProductFilter { get; } = new(ActivityService);

	/// <summary>Gets an instance of <see cref="ApplicationSettingsViewModel"/> for use during design time.</summary>
	public static ApplicationSettingsViewModel ApplicationSettingsViewModel { get; } =
		GetServiceProvider().GetRequiredService<ApplicationSettingsViewModel>();

	/// <summary>Gets an instance of <see cref="PreferencesViewModel"/> for use during design time.</summary>
	public static PreferencesViewModel PreferencesViewModel { get; } =
		GetServiceProvider().GetRequiredService<PreferencesViewModel>();

	/// <summary>Gets an instance of <see cref="GnomeshadeConfigurationViewModel"/> for use during design time.</summary>
	public static GnomeshadeConfigurationViewModel GnomeshadeConfigurationViewModel { get; } =
		GetServiceProvider().GetRequiredService<GnomeshadeConfigurationViewModel>();

	/// <summary>Gets an instance of <see cref="AuthenticationConfigurationViewModel"/> for use during design time.</summary>
	public static AuthenticationConfigurationViewModel AuthenticationConfigurationViewModel { get; } =
		GetServiceProvider().GetRequiredService<AuthenticationConfigurationViewModel>();

	/// <summary>Gets an instance of <see cref="ConfigurationWizardViewModel"/> for use during design time.</summary>
	public static ConfigurationWizardViewModel ConfigurationWizardViewModel { get; } =
		GetServiceProvider().GetRequiredService<ConfigurationWizardViewModel>();

	/// <summary>Gets an instance of <see cref="OwnerUpsertionViewModel"/> for use during design time.</summary>
	public static OwnerUpsertionViewModel OwnerUpsertionViewModel { get; } =
		InitializeViewModel<OwnerUpsertionViewModel>(new(ActivityService, GnomeshadeClient, Guid.Empty));

	/// <summary>Gets an instance of <see cref="OwnerViewModel"/> for use during design time.</summary>
	public static OwnerViewModel OwnerViewModel { get; } =
		InitializeViewModel<OwnerViewModel, OwnerRow, OwnerUpsertionViewModel>(new(ActivityService, GnomeshadeClient));

	[UnconditionalSuppressMessage(
		"Trimming",
		"IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
		Justification = "Configuration can be safely trimmed if it is not referenced")]
	private static IServiceProvider GetServiceProvider()
	{
		var serviceCollection = new ServiceCollection();
		var configuration = new ConfigurationBuilder()
			.AddEnvironmentVariables()
			.Build();

		serviceCollection
			.AddOptions<UserConfiguration>()
			.Bind(configuration);

		serviceCollection
			.AddSingleton<IConfiguration>(configuration)
			.AddSingleton<UserConfigurationWriter>()
			.AddSingleton<IClock>(SystemClock.Instance)
			.AddSingleton(DateTimeZoneProviders.Tzdb)
			.AddSingleton<IDialogService, DesignTimeDialogService>()
			.AddSingleton<Lazy<IManagedNotificationManager>>(_ => new(() => new WindowNotificationManager(null)))
			.AddSingleton<IActivityService, ActivityService>()
			.AddTransient<ApplicationSettingsViewModel>()
			.AddTransient<PreferencesViewModel>()
			.AddTransient<GnomeshadeConfigurationViewModel>()
			.AddTransient<AuthenticationConfigurationViewModel>()
			.AddTransient<ConfigurationWizardViewModel>()
			.AddTransient<ILogger<PreferencesViewModel>>(_ => NullLogger<PreferencesViewModel>.Instance)
			.AddTransient<IGnomeshadeClient>(_ => GnomeshadeClient)
			.AddHttpClient();

		return serviceCollection.BuildServiceProvider();
	}

	private static TViewModel InitializeViewModel<TViewModel>(TViewModel viewModel)
		where TViewModel : ViewModelBase
	{
		viewModel.RefreshAsync().GetAwaiter().GetResult();
		return viewModel;
	}

	private static TOverviewViewModel InitializeViewModel<TOverviewViewModel, TRow, TUpsertion>(TOverviewViewModel viewModel)
		where TOverviewViewModel : OverviewViewModel<TRow, TUpsertion>
		where TRow : PropertyChangedBase
		where TUpsertion : UpsertionViewModel?
	{
		viewModel.RefreshAsync().GetAwaiter().GetResult();
		viewModel.Details?.RefreshAsync().GetAwaiter().GetResult();
		return viewModel;
	}
}
