// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Reactive;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ReactiveUI;

using Tracking.Finance.Interfaces.WebApi.Client;
using Tracking.Finance.Interfaces.WebApi.V1_0.Authentication;

namespace Tracking.Finance.Interfaces.Desktop.ViewModels
{
	public class LoginViewModel : ViewModelBase
	{
		private readonly MainWindowViewModel _mainWindow;
		private readonly IFinanceClient _financeClient;
		private string? _errorMessage;
		private string? _username;
		private string? _password;

		/// <summary>
		/// Initializes a new instance of the <see cref="LoginViewModel"/> class.
		/// </summary>
		[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
		public LoginViewModel()
			: this(new(), new FinanceClient())
		{
		}

		public LoginViewModel(MainWindowViewModel mainWindow, IFinanceClient financeClient)
		{
			_mainWindow = mainWindow;
			_financeClient = financeClient;

			CanLogIn = this.WhenAnyValue(
				model => model.Username,
				model => model.Password,
				(user, pass) => !string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass));

			IsErrorMessageVisible = this.WhenAnyValue(
				model => model.ErrorMessage,
				message => !string.IsNullOrWhiteSpace(message));

			LogInCommand = ReactiveCommand.CreateFromTask(LogInAsync, CanLogIn);
		}

		/// <summary>
		/// Gets or sets the error message to display after a failed log in attempt.
		/// </summary>
		public string? ErrorMessage
		{
			get => _errorMessage;
			set => this.RaiseAndSetIfChanged(ref _errorMessage, value, nameof(ErrorMessage));
		}

		/// <summary>
		/// Gets a value indicating whether or not the <see cref="ErrorMessage"/> should be visible.
		/// </summary>
		public IObservable<bool> IsErrorMessageVisible { get; }

		/// <summary>
		/// Gets or sets the username entered by the user.
		/// </summary>
		public string? Username
		{
			get => _username;
			set => this.RaiseAndSetIfChanged(ref _username, value, nameof(Username));
		}

		/// <summary>
		/// Gets or sets the password entered by the user.
		/// </summary>
		public string? Password
		{
			get => _password;
			set => this.RaiseAndSetIfChanged(ref _password, value, nameof(Password));
		}

		/// <summary>
		/// Gets a value indicating whether or not the user can log in.
		/// </summary>
		public IObservable<bool> CanLogIn { get; }

		/// <summary>
		/// Gets the <see cref="ReactiveCommand{TParam,TResult}"/> for authenticating using the specified credentials.
		/// </summary>
		public ReactiveCommand<Unit, Unit> LogInCommand { get; }

		private async Task LogInAsync()
		{
			try
			{
				ErrorMessage = string.Empty;

				var loginModel = new LoginModel { Username = Username!, Password = Password! };
				_ = await _financeClient.Login(loginModel).ConfigureAwait(false);
				_mainWindow.ActiveView = new TransactionViewModel(_mainWindow);
			}
			catch (Exception exception)
			{
				ErrorMessage = exception.Message;
			}
		}
	}
}
