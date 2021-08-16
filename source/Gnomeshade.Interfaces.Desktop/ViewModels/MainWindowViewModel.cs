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
	/// <summary>
	/// A container view which manages navigation and the currently active view.
	/// </summary>
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
		public static void Exit()
		{
			var desktopLifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
			desktopLifetime.Shutdown();
		}

		/// <summary>
		/// Logs out the current user.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task LogOut()
		{
			await _gnomeshadeClient.LogOutAsync().ConfigureAwait(false);
			SwitchToLogin();
		}

		/// <summary>
		/// Switches <see cref="ActiveView"/> to <see cref="AccountCreationViewModel"/>.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

		/// <summary>
		/// Switches <see cref="ActiveView"/> to <see cref="TransactionCreationViewModel"/>.
		/// </summary>
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

		/// <summary>
		/// Switches <see cref="ActiveView"/> to <see cref="ProductCreationViewModel"/>.
		/// </summary>
		/// <param name="productId">The id of the product to edit.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task CreateProductAsync(Guid? productId = null)
		{
			if (ActiveView is ProductCreationViewModel)
			{
				return;
			}

			var productCreationViewModel = await ProductCreationViewModel.CreateAsync(_gnomeshadeClient, productId);
			productCreationViewModel.ProductCreated += OnProductCreated;

			ActiveView = productCreationViewModel;
		}

		/// <summary>
		/// Switches <see cref="ActiveView"/> to <see cref="UnitCreationView"/>.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
			importViewModel.ProductSelected += OnProductSelected;
			importViewModel.TransactionItemSelected += OnTransactionItemSelected;
			ActiveView = importViewModel;
		}

		/// <summary>
		/// Switches <see cref="ActiveView"/> to <see cref="TransactionItemCreationViewModel"/>.
		/// </summary>
		/// <param name="itemId">The id of the transaction item to edit.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task CreateTransactionItem(Guid itemId)
		{
			if (ActiveView is TransactionItemCreationViewModel)
			{
				return;
			}

			var itemCreationViewModel = await TransactionItemCreationViewModel.CreateAsync(_gnomeshadeClient, itemId);
			itemCreationViewModel.TransactionItemCreated += OnTransactionItemCreated;
			ActiveView = itemCreationViewModel;
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
			Task.Run(SwitchToTransactionOverviewAsync).Wait();
		}

		private void OnUserLoggedIn(object? sender, EventArgs e)
		{
			Task.Run(SwitchToTransactionOverviewAsync).Wait();
		}

		private void OnAccountCreated(object? sender, AccountCreatedEventArgs e)
		{
			Task.Run(SwitchToTransactionOverviewAsync).Wait();
		}

		private void OnProductCreated(object? sender, ProductCreatedEventArgs e)
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

		private void OnUnitCreated(object? sender, UnitCreatedEventArgs e)
		{
			Task.Run(SwitchToTransactionOverviewAsync).Wait();
		}

		private void OnTransactionSelected(object? sender, TransactionSelectedEventArgs e)
		{
			Task.Run(() => SwitchToTransactionDetailAsync(e.TransactionId)).Wait();
		}

		private void OnProductSelected(object? sender, ProductSelectedEventArgs e)
		{
			Task.Run(() => CreateProductAsync(e.ProductId)).Wait();
		}

		private void OnTransactionItemSelected(object? sender, TransactionItemSelectedEventArgs args)
		{
			Task.Run(() => CreateTransactionItem(args.ItemId)).Wait();
		}

		private void OnTransactionItemCreated(object? sender, TransactionItemCreatedEventArgs args)
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

		private void Unsubscribe(ViewModelBase viewModel)
		{
			switch (viewModel)
			{
				case AccountCreationViewModel accountCreationViewModel:
					accountCreationViewModel.AccountCreated -= OnAccountCreated;
					break;

				case AccountViewModel:
					break;

				case ImportViewModel importViewModel:
					importViewModel.ProductSelected -= OnProductSelected;
					importViewModel.TransactionItemSelected -= OnTransactionItemSelected;
					break;

				case LoginViewModel loginViewModel:
					loginViewModel.UserLoggedIn -= OnUserLoggedIn;
					break;

				case ProductCreationViewModel productCreationViewModel:
					productCreationViewModel.ProductCreated -= OnProductCreated;
					break;

				case TransactionCreationViewModel transactionCreationViewModel:
					transactionCreationViewModel.TransactionCreated -= OnTransactionCreated;
					break;

				case TransactionDetailViewModel:
					break;

				case TransactionItemCreationViewModel transactionItemCreationViewModel:
					transactionItemCreationViewModel.TransactionItemCreated -= OnTransactionItemCreated;
					break;

				case TransactionViewModel transactionViewModel:
					transactionViewModel.TransactionSelected -= OnTransactionSelected;
					break;

				case UnitCreationViewModel unitCreationViewModel:
					unitCreationViewModel.UnitCreated -= OnUnitCreated;
					break;
			}
		}
	}
}
