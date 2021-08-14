// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.Desktop.Views;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.Desktop.ViewModels
{
	public sealed class LoginViewModel : ViewModelBase<LoginView>
	{
		private readonly IGnomeshadeClient _gnomeshadeClient;

		private string? _errorMessage;
		private string? _username;
		private string? _password;

		/// <summary>
		/// Initializes a new instance of the <see cref="LoginViewModel"/> class.
		/// </summary>
		[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
		public LoginViewModel()
			: this(new GnomeshadeClient())
		{
		}

		public LoginViewModel(IGnomeshadeClient gnomeshadeClient)
		{
			_gnomeshadeClient = gnomeshadeClient;
		}

		/// <summary>
		/// Raised when a user has successfully logged in.
		/// </summary>
		public event EventHandler? UserLoggedIn;

		/// <summary>
		/// Gets or sets the error message to display after a failed log in attempt.
		/// </summary>
		public string? ErrorMessage
		{
			get => _errorMessage;
			set => SetAndNotifyWithGuard(ref _errorMessage, value, nameof(ErrorMessage), nameof(IsErrorMessageVisible));
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
			set => SetAndNotifyWithGuard(ref _username, value, nameof(Username), nameof(CanLogIn));
		}

		/// <summary>
		/// Gets or sets the password entered by the user.
		/// </summary>
		public string? Password
		{
			get => _password;
			set => SetAndNotifyWithGuard(ref _password, value, nameof(Password), nameof(CanLogIn));
		}

		/// <summary>
		/// Gets a value indicating whether or not the user can log in.
		/// </summary>
		public bool CanLogIn => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

		/// <summary>
		/// Attempts to log in using the specified credentials.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Unexpected <see cref="LoginResult"/> type.</exception>
		public async Task LogInAsync()
		{
			ErrorMessage = string.Empty;

			var loginModel = new Login { Username = Username!, Password = Password! };
			var loginResult = await _gnomeshadeClient.LogInAsync(loginModel).ConfigureAwait(false);

			switch (loginResult)
			{
				case FailedLogin failedLogin:
					ErrorMessage = failedLogin.Message;
					break;

				case SuccessfulLogin:
					OnUserLoggedIn();
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(loginResult));
			}
		}

		private void OnUserLoggedIn()
		{
			UserLoggedIn?.Invoke(this, EventArgs.Empty);
		}
	}
}
