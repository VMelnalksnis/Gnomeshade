// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Tracking.Finance.Interfaces.WebApi.Client;
using Tracking.Finance.Interfaces.WebApi.Client.Login;
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
		}

		/// <summary>
		/// Gets or sets the error message to display after a failed log in attempt.
		/// </summary>
		public string? ErrorMessage
		{
			get => _errorMessage;
			set
			{
				_errorMessage = value;
				OnPropertiesChanged(nameof(ErrorMessage), nameof(IsErrorMessageVisible));
			}
		}

		/// <summary>
		/// Gets a value indicating whether or not the <see cref="ErrorMessage"/> should be visible.
		/// </summary>
		public bool IsErrorMessageVisible => !string.IsNullOrWhiteSpace(ErrorMessage);

		/// <summary>
		/// Gets or sets the username entered by the user.
		/// </summary>
		public string? Username
		{
			get => _username;
			set
			{
				_username = value;
				OnPropertiesChanged(nameof(Username), nameof(CanLogIn));
			}
		}

		/// <summary>
		/// Gets or sets the password entered by the user.
		/// </summary>
		public string? Password
		{
			get => _password;
			set
			{
				_password = value;
				OnPropertiesChanged(nameof(Password), nameof(CanLogIn));
			}
		}

		/// <summary>
		/// Gets a value indicating whether or not the user can log in.
		/// </summary>
		public bool CanLogIn => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

		public async Task LogInAsync()
		{
			ErrorMessage = string.Empty;

			var loginModel = new LoginModel { Username = Username!, Password = Password! };
			var loginResult = await _financeClient.Login(loginModel).ConfigureAwait(false);

			switch (loginResult)
			{
				case SuccessfulLogin:
					_mainWindow.ActiveView = new TransactionViewModel(_mainWindow);
					break;

				case FailedLogin failedLogin:
					ErrorMessage = failedLogin.Message;
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(loginResult));
			}
		}
	}
}
