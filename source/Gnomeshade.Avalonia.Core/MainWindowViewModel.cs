// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

using Gnomeshade.Avalonia.Core.Accounts;
using Gnomeshade.Avalonia.Core.Authentication;
using Gnomeshade.Avalonia.Core.Configuration;
using Gnomeshade.Avalonia.Core.Counterparties;
using Gnomeshade.Avalonia.Core.Imports;
using Gnomeshade.Avalonia.Core.Products;
using Gnomeshade.Avalonia.Core.Reports;
using Gnomeshade.Avalonia.Core.Transactions;
using Gnomeshade.WebApi.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NodaTime;

namespace Gnomeshade.Avalonia.Core;

/// <summary>A container view which manages navigation and the currently active view.</summary>
public sealed class MainWindowViewModel : ViewModelBase
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IOptions<UserConfiguration> _userConfiguration;
	private readonly UserConfigurationWriter _userConfigurationWriter;
	private readonly UserConfigurationValidator _validator;
	private readonly IClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private ViewModelBase? _activeView;

	/// <summary>Initializes a new instance of the <see cref="MainWindowViewModel"/> class.</summary>
	/// <param name="serviceProvider">Dependency injection service provider.</param>
	public MainWindowViewModel(IServiceProvider serviceProvider)
		: base(serviceProvider.GetRequiredService<IActivityService>())
	{
		_serviceProvider = serviceProvider;

		_userConfiguration = serviceProvider.GetRequiredService<IOptions<UserConfiguration>>();
		_userConfigurationWriter = serviceProvider.GetRequiredService<UserConfigurationWriter>();
		_dateTimeZoneProvider = serviceProvider.GetRequiredService<IDateTimeZoneProvider>();
		_clock = serviceProvider.GetRequiredService<IClock>();

		_validator = serviceProvider.GetRequiredService<UserConfigurationValidator>();
	}

	/// <summary>
	/// Gets a value indicating whether it's possible to log out.
	/// </summary>
	public bool CanLogOut => ActiveView is not null and not LoginViewModel;

	/// <summary>
	/// Gets or sets the currently active view.
	/// </summary>
	public ViewModelBase? ActiveView
	{
		get => _activeView;
		set
		{
			Unsubscribe(ActiveView);
			if (ActiveView is not LoginViewModel)
			{
				PreviousView = ActiveView;
			}

			SetAndNotifyWithGuard(ref _activeView, value, nameof(ActiveView), nameof(CanLogOut));
		}
	}

	private ViewModelBase? PreviousView { get; set; }

	/// <summary>Safely stops the application.</summary>
	public static void Exit() => GetApplicationLifetime().Shutdown();

	/// <summary>Asynchronously initializes <see cref="ActiveView"/> after the window is displayed.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task InitializeActiveViewAsync()
	{
		if (ActiveView is not null)
		{
			return;
		}

		using var activity = BeginActivity();
		var isValid = await _validator.IsValid(_userConfiguration.Value);

		if (isValid)
		{
			await SwitchToLogin();
		}
		else
		{
			SwitchToSetup();
		}
	}

	/// <summary>Logs out the current user.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task LogOut()
	{
		var authenticationService = _serviceProvider.GetRequiredService<IAuthenticationService>();
		await authenticationService.Logout();
		await SwitchToLogin();
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="CounterpartyMergeViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task MergeCounterpartiesAsync() => SwitchTo<CounterpartyMergeViewModel>(() =>
	{
		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		return new(ActivityService, gnomeshadeClient);
	});

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="UnitCreationViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task CreateUnitAsync()
	{
		if (ActiveView is UnitCreationViewModel)
		{
			return;
		}

		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		var unitCreationViewModel = await UnitCreationViewModel.CreateAsync(ActivityService, gnomeshadeClient);
		unitCreationViewModel.Upserted += OnUnitUpserted;

		ActiveView = unitCreationViewModel;
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="CategoryViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToCategoriesAsync() => SwitchTo<CategoryViewModel>(() =>
	{
		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		return new(ActivityService, gnomeshadeClient);
	});

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="AccountViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToAccountOverviewAsync() => SwitchTo<AccountViewModel>(() =>
	{
		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		return new(ActivityService, gnomeshadeClient);
	});

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="CounterpartyViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToCounterpartiesAsync() => SwitchTo<CounterpartyViewModel>(() =>
	{
		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		return new(ActivityService, gnomeshadeClient);
	});

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="ImportViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToImportAsync() => SwitchTo<ImportViewModel>(() =>
	{
		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		return new(ActivityService, gnomeshadeClient);
	});

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="ProductViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToProductAsync() => SwitchTo<ProductViewModel>(() =>
	{
		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		return new(ActivityService, gnomeshadeClient, _dateTimeZoneProvider);
	});

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="UnitViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SwitchToUnitAsync()
	{
		if (ActiveView is UnitViewModel)
		{
			return;
		}

		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		var unitViewModel = await UnitViewModel.CreateAsync(ActivityService, gnomeshadeClient);
		ActiveView = unitViewModel;
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="CategoryReportViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToCategoryReportAsync() => SwitchTo<CategoryReportViewModel>(() =>
	{
		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		return new(ActivityService, gnomeshadeClient, _clock, _dateTimeZoneProvider);
	});

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="BalanceReportViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToBalanceReportAsync() => SwitchTo<BalanceReportViewModel>(() =>
	{
		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		return new(ActivityService, gnomeshadeClient, _clock, _dateTimeZoneProvider);
	});

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="ProductReportViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task SwitchToProductReportAsync() => SwitchTo<ProductReportViewModel>(() =>
	{
		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		return new(ActivityService, gnomeshadeClient, _clock, _dateTimeZoneProvider);
	});

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
		var authenticationService = _serviceProvider.GetRequiredService<IAuthenticationService>();
		var loginViewModel = new LoginViewModel(ActivityService, authenticationService);
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

	private void SwitchToSetup()
	{
		var initialSetupViewModel = new ApplicationSettingsViewModel(ActivityService, _userConfiguration, _userConfigurationWriter, _validator);
		initialSetupViewModel.Updated += InitialSetupViewModelOnUpdated;

		ActiveView = initialSetupViewModel;
	}

	private async void InitialSetupViewModelOnUpdated(object? sender, EventArgs e)
	{
		await SwitchToLogin();
	}

	private Task SwitchToTransactionOverviewAsync() => SwitchTo<TransactionViewModel>(() =>
	{
		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
		return new(ActivityService, gnomeshadeClient, dialogService, _clock, _dateTimeZoneProvider);
	});

	private Task SwitchTo<TViewModel>(Func<TViewModel> factory)
		where TViewModel : ViewModelBase
	{
		if (ActiveView is TViewModel)
		{
			return Task.CompletedTask;
		}

		var viewModel = factory();
		ActiveView = viewModel;
		return viewModel.RefreshAsync();
	}

	private async void OnUserLoggedIn(object? sender, EventArgs e)
	{
		await SwitchToTransactionOverviewAsync();
	}

	private async void OnAccountUpserted(object? sender, UpsertedEventArgs e)
	{
		await SwitchToTransactionOverviewAsync();
	}

	private async void OnProductUpserted(object? sender, UpsertedEventArgs e)
	{
		if (PreviousView is ImportViewModel importViewModel)
		{
			await importViewModel.RefreshAsync();
			ActiveView = importViewModel;
		}
		else
		{
			await SwitchToTransactionOverviewAsync();
		}
	}

	private async void OnUnitUpserted(object? sender, UpsertedEventArgs e)
	{
		await SwitchToTransactionOverviewAsync();
	}

	private void Unsubscribe(ViewModelBase? viewModel)
	{
		switch (viewModel)
		{
			case AccountUpsertionViewModel accountDetailViewModel:
				accountDetailViewModel.Upserted -= OnAccountUpserted;
				break;

			case LoginViewModel loginViewModel:
				loginViewModel.UserLoggedIn -= OnUserLoggedIn;
				break;

			case ProductUpsertionViewModel productCreationViewModel:
				productCreationViewModel.Upserted -= OnProductUpserted;
				break;

			case UnitCreationViewModel unitCreationViewModel:
				unitCreationViewModel.Upserted -= OnUnitUpserted;
				break;
		}
	}
}
