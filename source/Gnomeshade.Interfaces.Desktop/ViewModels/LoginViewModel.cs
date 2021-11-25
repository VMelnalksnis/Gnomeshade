// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.Desktop.Views;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Authentication;

using VMelnalksnis.OAuth2;
using VMelnalksnis.OAuth2.Responses;

namespace Gnomeshade.Interfaces.Desktop.ViewModels
{
	/// <summary>
	/// Form for authenticating the current user.
	/// </summary>
	public sealed class LoginViewModel : ViewModelBase<LoginView>
	{
		private readonly IGnomeshadeClient _gnomeshadeClient;
		private readonly IOAuth2Client _oAuth2Client;

		private string? _errorMessage;
		private string? _username;
		private string? _password;

		/// <summary>
		/// Initializes a new instance of the <see cref="LoginViewModel"/> class.
		/// </summary>
		/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
		/// <param name="oAuth2Client">OAuth2 provider API client.</param>
		public LoginViewModel(IGnomeshadeClient gnomeshadeClient, IOAuth2Client oAuth2Client)
		{
			_gnomeshadeClient = gnomeshadeClient;
			_oAuth2Client = oAuth2Client;
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
		/// Authenticate using an external identity provider.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task AuthenticateExternallyAsync()
		{
			var deviceAuthorizationResponse = await _oAuth2Client.StartDeviceFlowAsync();
			var processInfo = deviceAuthorizationResponse.GetProcessStartInfoForUserApproval();
			Process.Start(processInfo);

			var tokenResponse = await _oAuth2Client.GetDeviceFlowResultAsync(deviceAuthorizationResponse);
			await _gnomeshadeClient.SocialRegister(tokenResponse.AccessToken);
			OnUserLoggedIn();
		}

		/// <summary>
		/// Attempts to log in using the specified credentials.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Unexpected <see cref="LoginResult"/> type.</exception>
		public void LogInAsync()
		{
			ErrorMessage = string.Empty;

			var loginModel = new Login { Username = Username!, Password = Password! };
			var loginResult = _gnomeshadeClient.LogInAsync(loginModel).Result;

			switch (loginResult)
			{
				case FailedLogin failedLogin:
					ErrorMessage = failedLogin.Message;
					break;

				case SuccessfulLogin:
					OnUserLoggedIn();
					break;

				default:
					ErrorMessage = $"Unexpected login result: {loginResult}";
					break;
			}
		}

		private void OnUserLoggedIn()
		{
			UserLoggedIn?.Invoke(this, EventArgs.Empty);
		}
	}
}
