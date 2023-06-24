// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Authentication;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Authentication;

/// <summary>Form for authenticating the current user.</summary>
public sealed partial class LoginViewModel : ViewModelBase
{
	private readonly IAuthenticationService _authenticationService;

	/// <summary>Gets or sets the username entered by the user.</summary>
	[Notify]
	private string? _username;

	/// <summary>Gets or sets the password entered by the user.</summary>
	[Notify]
	private string? _password;

	/// <summary>Initializes a new instance of the <see cref="LoginViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="authenticationService">Service for handling authentication.</param>
	public LoginViewModel(IActivityService activityService, IAuthenticationService authenticationService)
		: base(activityService)
	{
		_authenticationService = authenticationService;
	}

	/// <summary>Raised when a user has successfully logged in.</summary>
	public event EventHandler? UserLoggedIn;

	/// <summary>Gets a value indicating whether or not the user can log in.</summary>
	public bool CanLogIn => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

	/// <summary>Authenticate using an external identity provider.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task AuthenticateExternallyAsync()
	{
		using var activity = BeginActivity("Logging in");
		try
		{
			await _authenticationService.SocialLogin();
			OnUserLoggedIn();
		}
		catch (Exception exception)
		{
			ActivityService.ShowErrorNotification(exception.Message);
		}
	}

	/// <summary>Attempts to log in using the specified credentials.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Unexpected <see cref="LoginResult"/> type.</exception>
	public async Task LogInAsync()
	{
		using var activity = BeginActivity("Logging in");

		var login = new Login { Username = Username!, Password = Password! };
		var loginResult = await _authenticationService.Login(login);

		switch (loginResult)
		{
			case FailedLogin failedLogin:
				ActivityService.ShowErrorNotification(failedLogin.Message);
				break;

			case SuccessfulLogin:
				OnUserLoggedIn();
				break;

			default:
				ActivityService.ShowErrorNotification($"Unexpected login result: {loginResult}");
				break;
		}
	}

	private void OnUserLoggedIn()
	{
		UserLoggedIn?.Invoke(this, EventArgs.Empty);
	}
}
