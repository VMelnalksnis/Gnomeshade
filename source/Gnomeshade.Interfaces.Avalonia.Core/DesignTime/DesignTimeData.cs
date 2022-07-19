// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.Avalonia.Core.Accounts;
using Gnomeshade.Interfaces.Avalonia.Core.Authentication;
using Gnomeshade.Interfaces.Avalonia.Core.Configuration;
using Gnomeshade.Interfaces.Avalonia.Core.Counterparties;
using Gnomeshade.Interfaces.Avalonia.Core.Imports;
using Gnomeshade.Interfaces.Avalonia.Core.Products;
using Gnomeshade.Interfaces.Avalonia.Core.Reports;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Controls;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Links;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Loans;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Purchases;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Transfers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NodaTime;

namespace Gnomeshade.Interfaces.Avalonia.Core.DesignTime;

/// <summary>Data needed only during design time.</summary>
public static class DesignTimeData
{
	private static IClock Clock => SystemClock.Instance;

	private static IDateTimeZoneProvider DateTimeZoneProvider => DateTimeZoneProviders.Tzdb;

	private static DesignTimeGnomeshadeClient GnomeshadeClient => new();

	private static IDialogService DialogService { get; } = new DesignTimeDialogService();

	private static IAuthenticationService AuthenticationService { get; } =
		new AuthenticationService(GnomeshadeClient);

	// Static member order is important due to initialization order
#pragma warning disable SA1202

	/// <summary>Gets an instance of <see cref="MainWindowViewModel"/> for use during design time.</summary>
	public static MainWindowViewModel MainWindowViewModel { get; } =
		new(GetServiceProvider());

	/// <summary>Gets an instance of <see cref="AccountUpsertionViewModel"/> for use during design time.</summary>
	public static AccountUpsertionViewModel AccountUpsertionViewModel { get; } =
		AccountUpsertionViewModel.CreateAsync(GnomeshadeClient, Guid.Empty).Result;

	/// <summary>Gets an instance of <see cref="AccountViewModel"/> for use during design time.</summary>
	public static AccountViewModel AccountViewModel { get; } =
		AccountViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="CounterpartyViewModel"/> for use during design time.</summary>
	public static CounterpartyViewModel CounterpartyViewModel { get; } =
		CounterpartyViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="CounterpartyMergeViewModel"/> for use during design time.</summary>
	public static CounterpartyMergeViewModel CounterpartyMergeViewModel { get; } =
		CounterpartyMergeViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="CounterpartyUpdateViewModel"/> for use during design time.</summary>
	public static CounterpartyUpdateViewModel CounterpartyUpdateViewModel { get; } =
		CounterpartyUpdateViewModel.CreateAsync(GnomeshadeClient, Guid.Empty).Result;

	/// <summary>Gets an instance of <see cref="ImportViewModel"/> for use during design time.</summary>
	public static ImportViewModel ImportViewModel { get; } = new(GnomeshadeClient);

	/// <summary>Gets an instance of <see cref="LoginViewModel"/> for use during design time.</summary>
	public static LoginViewModel LoginViewModel { get; } = new(AuthenticationService);

	/// <summary>Gets an instance of <see cref="ProductCreationViewModel"/> for use during design time.</summary>
	public static ProductCreationViewModel ProductCreationViewModel { get; } =
		ProductCreationViewModel.CreateAsync(GnomeshadeClient, DateTimeZoneProvider).Result;

	/// <summary>Gets an instance of <see cref="UnitCreationViewModel"/> for use during design time.</summary>
	public static UnitCreationViewModel UnitCreationViewModel { get; } =
		UnitCreationViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="CategoryCreationViewModel"/> for use during design time.</summary>
	public static CategoryCreationViewModel CategoryCreationViewModel { get; } =
		CategoryCreationViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="CategoryViewModel"/> for use during design time.</summary>
	public static CategoryViewModel CategoryViewModel { get; } =
		CategoryViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="TransactionProperties"/> for use during design time.</summary>
	public static TransactionProperties TransactionProperties { get; } = new();

	/// <summary>Gets an instance of <see cref="ProductViewModel"/> for use during design time.</summary>
	public static ProductViewModel ProductViewModel { get; } =
		ProductViewModel.CreateAsync(GnomeshadeClient, DateTimeZoneProvider).Result;

	/// <summary>Gets an instance of <see cref="UnitViewModel"/> for use during design time.</summary>
	public static UnitViewModel UnitViewModel { get; } =
		UnitViewModel.CreateAsync(GnomeshadeClient).Result;

	/// <summary>Gets an instance of <see cref="TransferUpsertionViewModel"/> for use during design time.</summary>
	public static TransferUpsertionViewModel TransferUpsertionViewModel =>
		Task.Run(async () =>
		{
			var viewModel = new TransferUpsertionViewModel(GnomeshadeClient, Guid.Empty, null);
			await viewModel.RefreshAsync().ConfigureAwait(false);
			return viewModel;
		}).Result;

	/// <summary>Gets an instance of <see cref="TransferViewModel"/> for use during design time.</summary>
	public static TransferViewModel TransferViewModel =>
		Task.Run(async () =>
		{
			var viewModel = new TransferViewModel(GnomeshadeClient, Guid.Empty);
			await viewModel.RefreshAsync().ConfigureAwait(false);
			await viewModel.Details.RefreshAsync().ConfigureAwait(false);
			return viewModel;
		}).Result;

	/// <summary>Gets an instance of <see cref="PurchaseUpsertionViewModel"/> for use during design time.</summary>
	public static PurchaseUpsertionViewModel PurchaseUpsertionViewModel =>
		Task.Run(async () =>
		{
			var viewModel = new PurchaseUpsertionViewModel(GnomeshadeClient, DialogService, DateTimeZoneProvider, Guid.Empty, null);
			await viewModel.RefreshAsync().ConfigureAwait(false);
			return viewModel;
		}).Result;

	/// <summary>Gets an instance of <see cref="PurchaseViewModel"/> for use during design time.</summary>
	public static PurchaseViewModel PurchaseViewModel =>
		Task.Run(async () =>
		{
			var viewModel = new PurchaseViewModel(GnomeshadeClient, DialogService, DateTimeZoneProvider, Guid.Empty);
			await viewModel.RefreshAsync().ConfigureAwait(false);
			await viewModel.Details.RefreshAsync().ConfigureAwait(false);
			return viewModel;
		}).Result;

	/// <summary>Gets an instance of <see cref="TransactionViewModel"/> for use during design time.</summary>
	public static TransactionViewModel TransactionViewModel =>
		Task.Run(async () =>
		{
			var viewModel = new TransactionViewModel(GnomeshadeClient, DialogService, Clock, DateTimeZoneProvider);
			await viewModel.RefreshAsync().ConfigureAwait(false);
			return viewModel;
		}).Result;

	/// <summary>Gets an instance of <see cref="TransactionFilter"/> for use during design time.</summary>
	public static TransactionFilter TransactionFilter { get; } =
		new() { FromDate = DateTimeOffset.Now, ToDate = DateTimeOffset.Now };

	/// <summary>Gets an instance of <see cref="TransactionUpsertionViewModel"/> for use during design time.</summary>
	public static TransactionUpsertionViewModel TransactionUpsertionViewModel =>
		Task.Run(async () =>
		{
			var viewModel = new TransactionUpsertionViewModel(GnomeshadeClient, DialogService, DateTimeZoneProvider, Guid.Empty);
			await viewModel.RefreshAsync().ConfigureAwait(false);
			return viewModel;
		}).Result;

	/// <summary>Gets an instance of <see cref="LinkUpsertionViewModel"/> for use during design time.</summary>
	public static LinkUpsertionViewModel LinkUpsertionViewModel =>
		Task.Run(async () =>
		{
			var viewModel = new LinkUpsertionViewModel(GnomeshadeClient, Guid.Empty, null);
			await viewModel.RefreshAsync().ConfigureAwait(false);
			return viewModel;
		}).Result;

	/// <summary>Gets an instance of <see cref="LinkViewModel"/> for use during design time.</summary>
	public static LinkViewModel LinkViewModel =>
		Task.Run(async () =>
		{
			var viewModel = new LinkViewModel(GnomeshadeClient, Guid.Empty);
			await viewModel.RefreshAsync().ConfigureAwait(false);
			return viewModel;
		}).Result;

	/// <summary>Gets an instance of <see cref="LoanUpsertionViewModel"/> for use during design time.</summary>
	public static LoanUpsertionViewModel LoanUpsertionViewModel =>
		Task.Run(async () =>
		{
			var viewModel = new LoanUpsertionViewModel(GnomeshadeClient, Guid.Empty, null);
			await viewModel.RefreshAsync().ConfigureAwait(false);
			return viewModel;
		}).Result;

	/// <summary>Gets an instance of <see cref="LoanViewModel"/> for use during design time.</summary>
	public static LoanViewModel LoanViewModel { get; } =
		LoanViewModel.CreateAsync(GnomeshadeClient, Guid.Empty).Result;

	/// <summary>Gets an instance of <see cref="CategoryReportViewModel"/> for use during design time.</summary>
	public static CategoryReportViewModel CategoryReportViewModel { get; } =
		CategoryReportViewModel.CreateAsync(GnomeshadeClient, Clock, DateTimeZoneProvider).Result;

	/// <summary>Gets an instance of <see cref="TransactionSummary"/> for use during design time.</summary>
	public static TransactionSummary TransactionSummary { get; } = new();

	/// <summary>Gets an instance of <see cref="ProductFilter"/> for use during design time.</summary>
	public static ProductFilter ProductFilter { get; } = new();

	/// <summary>Gets an instance of <see cref="ApplicationSettingsViewModel"/> for use during design time.</summary>
	public static ApplicationSettingsViewModel ApplicationSettingsViewModel { get; } =
		GetServiceProvider().GetRequiredService<ApplicationSettingsViewModel>();

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
			.AddSingleton<UserConfigurationValidator>()
			.AddSingleton<IDialogService, DesignTimeDialogService>()
			.AddTransient<ApplicationSettingsViewModel>()
			.AddHttpClient();

		return serviceCollection.BuildServiceProvider();
	}
}
