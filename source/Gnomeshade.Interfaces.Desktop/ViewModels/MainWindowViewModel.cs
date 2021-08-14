// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

using Gnomeshade.Interfaces.Desktop.ViewModels.Events;
using Gnomeshade.Interfaces.Desktop.Views;
using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Desktop.ViewModels
{
	public sealed class MainWindowViewModel : ViewModelBase<MainWindow>
	{
		private readonly IGnomeshadeClient _gnomeshadeClient;
		private ViewModelBase _activeView = null!;

		/// <summary>
		/// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
		/// </summary>
		public MainWindowViewModel()
		{
			_gnomeshadeClient = new GnomeshadeClient();
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
			await _gnomeshadeClient.LogOutAsync().ConfigureAwait(false);
			SwitchToLogin();
		}

		public async Task CreateAccountAsync()
		{
			if (ActiveView is AccountCreationViewModel)
			{
				return;
			}

			var accountCreationViewModel = await AccountCreationViewModel.CreateAsync(_gnomeshadeClient);
			accountCreationViewModel.AccountCreated += OnAccountCreated;

			ActiveView = accountCreationViewModel;
		}

		public void CreateTransaction()
		{
			if (ActiveView is TransactionCreationViewModel)
			{
				return;
			}

			var transactionCreationViewModel = new TransactionCreationViewModel(_gnomeshadeClient);
			transactionCreationViewModel.TransactionCreated += OnTransactionCreated;

			ActiveView = transactionCreationViewModel;
		}

		public async Task CreateProductAsync()
		{
			if (ActiveView is ProductCreationViewModel)
			{
				return;
			}

			var productCreationViewModel = await ProductCreationViewModel.CreateAsync(_gnomeshadeClient);
			productCreationViewModel.ProductCreated += OnProductCreated;

			ActiveView = productCreationViewModel;
		}

		public async Task CreateUnitAsync()
		{
			if (ActiveView is UnitCreationViewModel)
			{
				return;
			}

			var unitCreationViewModel = await UnitCreationViewModel.CreateAsync(_gnomeshadeClient);
			unitCreationViewModel.UnitCreated += OnUnitCreated;

			ActiveView = unitCreationViewModel;
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

		private void SwitchToLogin()
		{
			var loginViewModel = new LoginViewModel(_gnomeshadeClient);
			loginViewModel.UserLoggedIn += OnUserLoggedIn;

			ActiveView = loginViewModel;
		}

		private async Task SwitchToTransactionOverviewAsync()
		{
			var transactionViewModel = await TransactionViewModel.CreateAsync(_gnomeshadeClient);
			transactionViewModel.TransactionSelected += OnTransactionSelected;
			ActiveView = transactionViewModel;
		}

		private async Task SwitchToTransactionDetailAsync(Guid id)
		{
			var transactionDetailViewModel =
				await TransactionDetailViewModel.CreateAsync(_gnomeshadeClient, id).ConfigureAwait(false);
			ActiveView = transactionDetailViewModel;
		}

		private void OnTransactionCreated(object? sender, TransactionCreatedEventArgs e)
		{
			SwitchToTransactionOverviewAsync().Wait();
		}

		private void OnUserLoggedIn(object? sender, EventArgs e)
		{
			SwitchToTransactionOverviewAsync().Wait();
		}

		private void OnAccountCreated(object? sender, AccountCreatedEventArgs e)
		{
			SwitchToTransactionOverviewAsync().Wait();
		}

		private void OnProductCreated(object? sender, ProductCreatedEventArgs e)
		{
			SwitchToTransactionOverviewAsync().Wait();
		}

		private void OnUnitCreated(object? sender, UnitCreatedEventArgs e)
		{
			SwitchToTransactionOverviewAsync().Wait();
		}

		private void OnTransactionSelected(object? sender, TransactionSelectedEventArgs e)
		{
			SwitchToTransactionDetailAsync(e.TransactionId).Wait();
		}
	}
}
