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

		public void CreateAccount()
		{
			if (ActiveView is AccountCreationViewModel)
			{
				return;
			}

			var accountCreationViewModel = new AccountCreationViewModel(_financeClient);
			accountCreationViewModel.AccountCreated += OnAccountCreated;

			ActiveView = accountCreationViewModel;
		}

		public void CreateTransaction()
		{
			if (ActiveView is TransactionCreationViewModel)
			{
				return;
			}

			var transactionCreationViewModel = new TransactionCreationViewModel(_financeClient);
			transactionCreationViewModel.TransactionCreated += OnTransactionCreated;

			ActiveView = transactionCreationViewModel;
		}

		public void CreateProduct()
		{
			if (ActiveView is ProductCreationViewModel)
			{
				return;
			}

			var productCreationViewModel = new ProductCreationViewModel(_financeClient);
			productCreationViewModel.ProductCreated += OnProductCreated;

			ActiveView = productCreationViewModel;
		}

		public void CreateUnit()
		{
			if (ActiveView is UnitCreationViewModel)
			{
				return;
			}

			var unitCreationViewModel = new UnitCreationViewModel(_financeClient);
			unitCreationViewModel.UnitCreated += OnUnitCreated;

			ActiveView = unitCreationViewModel;
		}

		private void SwitchToLogin()
		{
			var loginViewModel = new LoginViewModel(_financeClient);
			loginViewModel.UserLoggedIn += OnUserLoggedIn;

			ActiveView = loginViewModel;
		}

		private void SwitchToTransactionOverview()
		{
			var transactionViewModel = new TransactionViewModel(_financeClient);
			ActiveView = transactionViewModel;
		}

		private void OnTransactionCreated(object? sender, TransactionCreatedEventArgs e)
		{
			SwitchToTransactionOverview();
		}

		private void OnUserLoggedIn(object? sender, EventArgs e)
		{
			SwitchToTransactionOverview();
		}

		private void OnAccountCreated(object? sender, AccountCreatedEventArgs e)
		{
			SwitchToTransactionOverview();
		}

		private void OnProductCreated(object? sender, ProductCreatedEventArgs e)
		{
			SwitchToTransactionOverview();
		}

		private void OnUnitCreated(object? sender, UnitCreatedEventArgs e)
		{
			SwitchToTransactionOverview();
		}
	}
}
