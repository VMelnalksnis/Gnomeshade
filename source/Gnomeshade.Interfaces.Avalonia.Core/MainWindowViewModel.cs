// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

using Gnomeshade.Interfaces.Avalonia.Core.Accounts;
using Gnomeshade.Interfaces.Avalonia.Core.Authentication;
using Gnomeshade.Interfaces.Avalonia.Core.Counterparties;
using Gnomeshade.Interfaces.Avalonia.Core.Imports;
using Gnomeshade.Interfaces.Avalonia.Core.Products;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions;
using Gnomeshade.Interfaces.WebApi.Client;

using NodaTime;

namespace Gnomeshade.Interfaces.Avalonia.Core;

/// <summary>A container view which manages navigation and the currently active view.</summary>
public sealed class MainWindowViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;
	private readonly IAuthenticationService _authenticationService;
	private readonly IClock _clock;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	private ViewModelBase _activeView = null!;

	/// <summary>Initializes a new instance of the <see cref="MainWindowViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="authenticationService">OAuth2 provider API client.</param>
	/// <param name="clock">Clock which can provide the current instant.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	public MainWindowViewModel(
		IGnomeshadeClient gnomeshadeClient,
		IAuthenticationService authenticationService,
		IClock clock,
		IDateTimeZoneProvider dateTimeZoneProvider)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_authenticationService = authenticationService;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_clock = clock;

		SwitchToLogin();
	}

	/// <summary>
	/// Gets a value indicating whether it's possible to log out.
	/// </summary>
	public bool CanLogOut => ActiveView is not LoginViewModel;

	/// <summary>
	/// Gets or sets the currently active view.
	/// </summary>
	public ViewModelBase ActiveView
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

	/// <summary>
	/// Safely stops the application.
	/// </summary>
	public static void Exit() => GetApplicationLifetime().Shutdown();

	/// <summary>
	/// Logs out the current user.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task LogOut()
	{
		await _authenticationService.Logout().ConfigureAwait(false);
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

		var counterpartyMergeViewModel = await CounterpartyMergeViewModel.CreateAsync(_gnomeshadeClient);
		ActiveView = counterpartyMergeViewModel;
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="UnitCreationViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task CreateUnitAsync()
	{
		if (ActiveView is UnitCreationViewModel)
		{
			return;
		}

		var unitCreationViewModel = await UnitCreationViewModel.CreateAsync(_gnomeshadeClient);
		unitCreationViewModel.Upserted += OnUnitUpserted;

		ActiveView = unitCreationViewModel;
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="CategoryCreationViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SwitchToCategoriesAsync()
	{
		if (ActiveView is CategoryViewModel)
		{
			return;
		}

		var categoryViewModel = await CategoryViewModel.CreateAsync(_gnomeshadeClient);
		ActiveView = categoryViewModel;
	}

	/// <summary>
	/// Switches <see cref="ActiveView"/> to <see cref="AccountViewModel"/>.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SwitchToAccountOverviewAsync()
	{
		if (ActiveView is AccountViewModel)
		{
			return;
		}

		var accountViewModel = await AccountViewModel.CreateAsync(_gnomeshadeClient).ConfigureAwait(false);
		ActiveView = accountViewModel;
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="CounterpartyViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SwitchToCounterpartiesAsync()
	{
		if (ActiveView is CounterpartyViewModel)
		{
			return;
		}

		var counterpartyViewModel = await CounterpartyViewModel.CreateAsync(_gnomeshadeClient).ConfigureAwait(false);
		ActiveView = counterpartyViewModel;
	}

	/// <summary>
	/// Switches <see cref="ActiveView"/> to <see cref="ImportViewModel"/>.
	/// </summary>
	public void SwitchToImport()
	{
		if (ActiveView is ImportViewModel)
		{
			return;
		}

		var importViewModel = new ImportViewModel(_gnomeshadeClient);
		ActiveView = importViewModel;
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="ProductViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SwitchToProductAsync()
	{
		if (ActiveView is ProductViewModel)
		{
			return;
		}

		var productViewModel = await ProductViewModel.CreateAsync(_gnomeshadeClient).ConfigureAwait(false);
		ActiveView = productViewModel;
	}

	/// <summary>Switches <see cref="ActiveView"/> to <see cref="UnitViewModel"/>.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task SwitchToUnitAsync()
	{
		if (ActiveView is UnitViewModel)
		{
			return;
		}

		var unitViewModel = await UnitViewModel.CreateAsync(_gnomeshadeClient).ConfigureAwait(false);
		ActiveView = unitViewModel;
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
		var loginViewModel = new LoginViewModel(_authenticationService);
		loginViewModel.UserLoggedIn += OnUserLoggedIn;

		ActiveView = loginViewModel;
	}

	private async Task SwitchToTransactionOverviewAsync()
	{
		var transactionViewModel = await TransactionViewModel.CreateAsync(_gnomeshadeClient, _clock, _dateTimeZoneProvider);
		ActiveView = transactionViewModel;
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

	private void Unsubscribe(ViewModelBase viewModel)
	{
		switch (viewModel)
		{
			case AccountUpsertionViewModel accountDetailViewModel:
				accountDetailViewModel.Upserted -= OnAccountUpserted;
				break;

			case LoginViewModel loginViewModel:
				loginViewModel.UserLoggedIn -= OnUserLoggedIn;
				break;

			case ProductCreationViewModel productCreationViewModel:
				productCreationViewModel.Upserted -= OnProductUpserted;
				break;

			case UnitCreationViewModel unitCreationViewModel:
				unitCreationViewModel.Upserted -= OnUnitUpserted;
				break;
		}
	}
}
