// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

using Gnomeshade.Avalonia.Core.Accesses;
using Gnomeshade.Avalonia.Core.Accounts;
using Gnomeshade.Avalonia.Core.Authentication;
using Gnomeshade.Avalonia.Core.Configuration;
using Gnomeshade.Avalonia.Core.Counterparties;
using Gnomeshade.Avalonia.Core.Imports;
using Gnomeshade.Avalonia.Core.Products;
using Gnomeshade.Avalonia.Core.Reports;
using Gnomeshade.Avalonia.Core.Transactions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core;

/// <summary>A container view which manages navigation and the currently active view.</summary>
public sealed partial class MainWindowViewModel : ViewModelBase
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IOptionsMonitor<UserConfiguration> _userConfigurationMonitor;

	/// <summary>Gets the currently active view.</summary>
	[Notify(Setter.Private)]
	private ViewModelBase? _activeView;

	/// <summary>Gets or sets the window height.</summary>
	[Notify]
	private int _windowHeight;

	/// <summary>Gets or sets the window width.</summary>
	[Notify]
	private int _windowWidth;

	/// <summary>Gets or sets the window state.</summary>
	[Notify]
	private WindowState _windowState;

	/// <summary>Initializes a new instance of the <see cref="MainWindowViewModel"/> class.</summary>
	/// <param name="serviceProvider">Dependency injection service provider.</param>
	public MainWindowViewModel(IServiceProvider serviceProvider)
		: base(serviceProvider.GetRequiredService<IActivityService>())
	{
		_serviceProvider = serviceProvider;
		_userConfigurationMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<UserConfiguration>>();

		var preferences = _userConfigurationMonitor.CurrentValue.Preferences;
		_windowHeight = preferences?.WindowHeight ?? 600;
		_windowWidth = preferences?.WindowWidth ?? 800;
		_windowState = preferences?.WindowState ?? WindowState.Normal;

		PropertyChanging += OnPropertyChanging;
	}

	/// <summary>Gets a value indicating whether it's possible to log out.</summary>
	public bool CanLogOut => ActiveView is not null and not LoginViewModel;

	/// <summary>Safely stops the application.</summary>
	public static void Exit() => GetApplicationLifetime().Shutdown();

	/// <summary>Asynchronously initializes <see cref="ActiveView"/> after the window is displayed.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task InitializeActiveViewAsync()
	{
		// The first notification does not show up, and subsequent calls work after some delay
		ActivityService.ShowNotification(new(null, null, expiration: TimeSpan.FromMilliseconds(1)));

		if (ActiveView is not null)
		{
			return;
		}

		var apiBaseAddress = _userConfigurationMonitor.CurrentValue.Gnomeshade?.BaseAddress;
		if (apiBaseAddress is not null)
		{
			await SwitchToLogin();
			return;
		}

		var configurationWizardViewModel = _serviceProvider.GetRequiredService<ConfigurationWizardViewModel>();
		configurationWizardViewModel.Completed += ConfigurationWizardViewModelOnUpdated;
		ActiveView = configurationWizardViewModel;
	}

	/// <summary>Logs out the current user.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task LogOut()
	{
		var authenticationService = _serviceProvider.GetRequiredService<IAuthenticationService>();
		try
		{
			await authenticationService.Logout();
			await SwitchToLogin();
		}
		catch (Exception exception)
		{
			ActivityService.ShowErrorNotification(exception.Message);
		}
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="CounterpartyMergeViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task MergeCounterpartiesAsync() => SwitchTo<CounterpartyMergeViewModel>();

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="CategoryViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToCategoriesAsync() => SwitchTo<CategoryViewModel>();

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="AccountViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToAccountOverviewAsync() => SwitchTo<AccountViewModel>();

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="AccountViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToAccessOverviewAsync() => SwitchTo<OwnerViewModel>();

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="CounterpartyViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToCounterpartiesAsync() => SwitchTo<CounterpartyViewModel>();

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="ImportViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToImportAsync() => SwitchTo<ImportViewModel>();

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="ProductViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToProductAsync() => SwitchTo<ProductViewModel>();

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="UnitViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToUnitAsync() => SwitchTo<UnitViewModel>();

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="CategoryReportViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToCategoryReportAsync() => SwitchTo<CategoryReportViewModel>();

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="BalanceReportViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToBalanceReportAsync() => SwitchTo<BalanceReportViewModel>();

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="ProductReportViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToProductReportAsync() => SwitchTo<ProductReportViewModel>();

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="ApplicationSettingsViewModel"/>.</summary>
	public void SwitchToSetup()
	{
		var applicationSettingsViewModel = _serviceProvider.GetRequiredService<ApplicationSettingsViewModel>();
		applicationSettingsViewModel.Updated += InitialSetupViewModelOnUpdated;

		ActiveView = applicationSettingsViewModel;
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="PreferencesViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToPreferences() => SwitchTo<PreferencesViewModel>();

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="TransactionViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToTransactionOverviewAsync() => SwitchTo<TransactionViewModel>();

	/// <summary>Event handler for <see cref="IClassicDesktopStyleApplicationLifetime.ShutdownRequested"/>.</summary>
	/// <param name="sender">The object that sent the event.</param>
	/// <param name="eventArgs">Event arguments.</param>
	public void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs eventArgs)
	{
		var configuration = _serviceProvider.GetRequiredService<IOptionsMonitor<UserConfiguration>>().CurrentValue;
		var writer = _serviceProvider.GetRequiredService<UserConfigurationWriter>();

		var preferences = configuration.Preferences ??= new();
		preferences.WindowHeight = WindowHeight;
		preferences.WindowWidth = WindowWidth;
		preferences.WindowState = WindowState;

		writer.Write(configuration);
	}

	private static IClassicDesktopStyleApplicationLifetime GetApplicationLifetime()
	{
		if (Application.Current is null)
		{
			throw CurrentApplicationIsNull();
		}

		if (Application.Current.ApplicationLifetime is null)
		{
			throw CurrentLifetimeIsNull();
		}

		return (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
	}

	private static Exception CurrentApplicationIsNull()
	{
		var message = $"Did not expected {typeof(Application)}.{nameof(Application.Current)} to be null";
		return new NullReferenceException(message);
	}

	private static Exception CurrentLifetimeIsNull()
	{
		var message =
			$"Did not expected {typeof(Application)}.{nameof(Application.Current)}.{nameof(Application.Current.ApplicationLifetime)} to be null";
		return new NullReferenceException(message);
	}

	private async Task SwitchToLogin()
	{
		var credentialStorageService = _serviceProvider.GetRequiredService<ICredentialStorageService>();
		var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
		loginViewModel.UserLoggedIn += OnUserLoggedIn;
		ActiveView = loginViewModel;

		if (credentialStorageService.TryGetRefreshToken(out _))
		{
			await loginViewModel.AuthenticateExternallyAsync();
		}
		else if (credentialStorageService.TryGetCredentials(out var username, out var password))
		{
			loginViewModel.Username = username;
			loginViewModel.Password = password;
			await loginViewModel.LogInAsync();
		}
	}

	private async void InitialSetupViewModelOnUpdated(object? sender, EventArgs e)
	{
		await SwitchToLogin();
	}

	private Task SwitchTo<TViewModel>()
		where TViewModel : ViewModelBase
	{
		if (ActiveView is TViewModel)
		{
			return Task.CompletedTask;
		}

		var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
		ActiveView = viewModel;
		return viewModel.RefreshAsync();
	}

	private async void OnUserLoggedIn(object? sender, EventArgs e)
	{
		await SwitchToTransactionOverviewAsync();
	}

	private async void ConfigurationWizardViewModelOnUpdated(object? sender, EventArgs e)
	{
		await SwitchToLogin();
	}

	private void OnPropertyChanging(object? sender, PropertyChangingEventArgs e)
	{
		if (e.PropertyName is not nameof(ActiveView))
		{
			return;
		}

		Unsubscribe(ActiveView);
	}

	private void Unsubscribe(ViewModelBase? viewModel)
	{
		if (viewModel is LoginViewModel loginViewModel)
		{
			loginViewModel.UserLoggedIn -= OnUserLoggedIn;
		}
		else if (viewModel is ApplicationSettingsViewModel applicationSettingsViewModel)
		{
			applicationSettingsViewModel.Updated -= InitialSetupViewModelOnUpdated;
		}
		else if (viewModel is ConfigurationWizardViewModel gnomeshadeConfigurationViewModel)
		{
			gnomeshadeConfigurationViewModel.Completed -= ConfigurationWizardViewModelOnUpdated;
		}
	}
}
