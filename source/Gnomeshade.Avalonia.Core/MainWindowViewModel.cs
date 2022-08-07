// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;

using Gnomeshade.Avalonia.Core.Accounts;
using Gnomeshade.Avalonia.Core.Authentication;
using Gnomeshade.Avalonia.Core.Configuration;
using Gnomeshade.Avalonia.Core.Counterparties;
using Gnomeshade.Avalonia.Core.Imports;
using Gnomeshade.Avalonia.Core.Products;
using Gnomeshade.Avalonia.Core.Reports;
using Gnomeshade.Avalonia.Core.Transactions;
using Gnomeshade.Interfaces.WebApi.Client;

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
	private Cursor _cursor;

	/// <summary>Initializes a new instance of the <see cref="MainWindowViewModel"/> class.</summary>
	/// <param name="serviceProvider">Dependency injection service provider.</param>
	public MainWindowViewModel(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;

		_userConfiguration = serviceProvider.GetRequiredService<IOptions<UserConfiguration>>();
		_userConfigurationWriter = serviceProvider.GetRequiredService<UserConfigurationWriter>();
		_dateTimeZoneProvider = serviceProvider.GetRequiredService<IDateTimeZoneProvider>();
		_clock = serviceProvider.GetRequiredService<IClock>();

		_cursor = Cursor.Default;

		PropertyChanged += OnPropertyChanged;

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

	/// <summary>Gets or sets the current cursor.</summary>
	public Cursor Cursor
	{
		get => _cursor;
		set => SetAndNotify(ref _cursor, value);
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

		IsBusy = true;
		var isValid = await _validator.IsValid(_userConfiguration.Value).ConfigureAwait(false);

		if (isValid)
		{
			SwitchToLogin();
		}
		else
		{
			SwitchToSetup();
		}

		IsBusy = false;
	}

	/// <summary>Logs out the current user.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task LogOut()
	{
		var authenticationService = _serviceProvider.GetRequiredService<IAuthenticationService>();
		await authenticationService.Logout().ConfigureAwait(false);
		SwitchToLogin();
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="CounterpartyMergeViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task MergeCounterpartiesAsync()
	{
		if (ActiveView is CounterpartyMergeViewModel)
		{
			return;
		}

		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		var counterpartyMergeViewModel = new CounterpartyMergeViewModel(gnomeshadeClient);
		ActiveView = counterpartyMergeViewModel;
		await counterpartyMergeViewModel.RefreshAsync();
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="UnitCreationViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task CreateUnitAsync()
	{
		if (ActiveView is UnitCreationViewModel)
		{
			return;
		}

		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		var unitCreationViewModel = await UnitCreationViewModel.CreateAsync(gnomeshadeClient);
		unitCreationViewModel.Upserted += OnUnitUpserted;

		ActiveView = unitCreationViewModel;
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="CategoryViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SwitchToCategoriesAsync()
	{
		if (ActiveView is CategoryViewModel)
		{
			return;
		}

		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		var categoryViewModel = new CategoryViewModel(gnomeshadeClient);
		ActiveView = categoryViewModel;
		await ActiveView.RefreshAsync();
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="AccountViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SwitchToAccountOverviewAsync()
	{
		if (ActiveView is AccountViewModel)
		{
			return;
		}

		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		var accountViewModel = new AccountViewModel(gnomeshadeClient);
		ActiveView = accountViewModel;
		await ActiveView.RefreshAsync();
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="CounterpartyViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SwitchToCounterpartiesAsync()
	{
		if (ActiveView is CounterpartyViewModel)
		{
			return;
		}

		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		var counterpartyViewModel = new CounterpartyViewModel(gnomeshadeClient);
		ActiveView = counterpartyViewModel;
		await ActiveView.RefreshAsync();
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="ImportViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SwitchToImportAsync()
	{
		if (ActiveView is ImportViewModel)
		{
			return;
		}

		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		var importViewModel = new ImportViewModel(gnomeshadeClient);
		ActiveView = importViewModel;
		await importViewModel.RefreshAsync();
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="ProductViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SwitchToProductAsync()
	{
		if (ActiveView is ProductViewModel)
		{
			return;
		}

		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		var productViewModel = new ProductViewModel(gnomeshadeClient, _dateTimeZoneProvider);
		ActiveView = productViewModel;
		await ActiveView.RefreshAsync();
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="UnitViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SwitchToUnitAsync()
	{
		if (ActiveView is UnitViewModel)
		{
			return;
		}

		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		var unitViewModel = await UnitViewModel.CreateAsync(gnomeshadeClient).ConfigureAwait(false);
		ActiveView = unitViewModel;
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="CategoryReportViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SwitchToCategoryReportAsync()
	{
		if (ActiveView is CategoryReportViewModel)
		{
			return;
		}

		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		var viewModel = new CategoryReportViewModel(gnomeshadeClient, _clock, _dateTimeZoneProvider);
		ActiveView = viewModel;
		await viewModel.RefreshAsync();
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

	private void SwitchToLogin()
	{
		var authenticationService = _serviceProvider.GetRequiredService<IAuthenticationService>();
		var loginViewModel = new LoginViewModel(authenticationService);
		loginViewModel.UserLoggedIn += OnUserLoggedIn;

		ActiveView = loginViewModel;
	}

	private void SwitchToSetup()
	{
		var initialSetupViewModel = new ApplicationSettingsViewModel(_userConfiguration, _userConfigurationWriter, _validator);
		initialSetupViewModel.Updated += InitialSetupViewModelOnUpdated;

		ActiveView = initialSetupViewModel;
	}

	private void InitialSetupViewModelOnUpdated(object? sender, EventArgs e)
	{
		SwitchToLogin();
	}

	private async Task SwitchToTransactionOverviewAsync()
	{
		var gnomeshadeClient = _serviceProvider.GetRequiredService<IGnomeshadeClient>();
		var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
		var transactionViewModel = new TransactionViewModel(gnomeshadeClient, dialogService, _clock, _dateTimeZoneProvider);
		ActiveView = transactionViewModel;
		await transactionViewModel.RefreshAsync();
	}

	private void OnUserLoggedIn(object? sender, EventArgs e)
	{
		Task.Run(SwitchToTransactionOverviewAsync).Wait();
	}

	private void OnAccountUpserted(object? sender, UpsertedEventArgs e)
	{
		Task.Run(SwitchToTransactionOverviewAsync).Wait();
	}

	private void OnProductUpserted(object? sender, UpsertedEventArgs e)
	{
		if (PreviousView is ImportViewModel importViewModel)
		{
			Task.Run(() => importViewModel.RefreshAsync()).Wait();
			ActiveView = importViewModel;
		}
		else
		{
			Task.Run(SwitchToTransactionOverviewAsync).Wait();
		}
	}

	private void OnUnitUpserted(object? sender, UpsertedEventArgs e)
	{
		Task.Run(SwitchToTransactionOverviewAsync).Wait();
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

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(ActiveView) && ActiveView is not null)
		{
			ActiveView.PropertyChanged += ActiveViewOnPropertyChanged;
		}
		else if (e.PropertyName is nameof(IsBusy))
		{
			Cursor = IsBusy ? new(StandardCursorType.Wait) : Cursor.Default;
		}
	}

	private void ActiveViewOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(IsBusy))
		{
			IsBusy = ActiveView?.IsBusy ?? false;
		}
	}
}
