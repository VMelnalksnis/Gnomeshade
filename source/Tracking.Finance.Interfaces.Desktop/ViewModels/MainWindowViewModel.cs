// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

using Tracking.Finance.Interfaces.Desktop.ViewModels.Events;
using Tracking.Finance.Interfaces.Desktop.Views;
using Tracking.Finance.Interfaces.WebApi.Client;

namespace Tracking.Finance.Interfaces.Desktop.ViewModels
{
	public sealed class MainWindowViewModel : ViewModelBase<MainWindow>
	{
		private readonly IFinanceClient _financeClient;
		private ViewModelBase _activeView = null!;
		private bool _canLogOut;

		/// <summary>
		/// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
		/// </summary>
		public MainWindowViewModel()
		{
			_financeClient = new FinanceClient();
			SwitchToLogin();
		}

		public bool CanLogOut => ActiveView is not LoginViewModel;

		/// <summary>
		/// Gets or sets the currently active view.
		/// </summary>
		public ViewModelBase ActiveView
		{
			get => _activeView;
			set => SetAndNotifyWithGuard(ref _activeView, value, nameof(ActiveView), nameof(CanLogOut));
		}

		/// <summary>
		/// Safely stops the application.
		/// </summary>
		public static void Exit()
		{
			var desktopLifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
			desktopLifetime.Shutdown();
		}

		public async Task LogOut()
		{
			await _financeClient.LogOutAsync().ConfigureAwait(false);
			SwitchToLogin();
		}

		private void SwitchToLogin()
		{
			var loginViewModel = new LoginViewModel(_financeClient);
			loginViewModel.UserLoggedIn += OnUserLoggedIn;

			ActiveView = loginViewModel;
		}

		private void OnUserLoggedIn(object? sender, EventArgs e)
		{
			var accountCreationViewModel = new AccountCreationViewModel(_financeClient);
			accountCreationViewModel.AccountCreated += OnAccountCreated;

			ActiveView = accountCreationViewModel;
		}

		private void OnAccountCreated(object? sender, AccountCreatedEventArgs e)
		{
			var transactionViewModel = new TransactionViewModel(_financeClient);
			ActiveView = transactionViewModel;
		}
	}
}
