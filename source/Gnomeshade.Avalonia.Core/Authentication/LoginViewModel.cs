// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Configuration;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Client.Results;
using Gnomeshade.WebApi.Models.Authentication;

using Microsoft.Extensions.Options;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Authentication;

/// <summary>Form for authenticating the current user.</summary>
public sealed partial class LoginViewModel : ViewModelBase
{
	private readonly IAuthenticationService _authenticationService;
	private readonly IDisposable? _configurationChangeScope;

	/// <summary>Gets or sets the username entered by the user.</summary>
	[Notify]
	private string? _username;

	/// <summary>Gets or sets the password entered by the user.</summary>
	[Notify]
	private string? _password;

	/// <summary>Gets or sets a value indicating whether external authentication is configured.</summary>
	[Notify]
	private bool _externalAuthenticationConfigured;

	/// <summary>Initializes a new instance of the <see cref="LoginViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="authenticationService">Service for handling authentication.</param>
	/// <param name="userConfiguration">Options monitor of user preferences.</param>
	public LoginViewModel(
		IActivityService activityService,
		IAuthenticationService authenticationService,
		IOptionsMonitor<UserConfiguration> userConfiguration)
		: base(activityService)
	{
		_authenticationService = authenticationService;
		_configurationChangeScope = userConfiguration.OnChange(configuration => ExternalAuthenticationConfigured = configuration.Oidc is not null);
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
			var result = await _authenticationService.SocialLogin();
			switch (result)
			{
				case LoggedIn:
					OnUserLoggedIn();
					break;

				case RequiresRegistration requiresRegistration:
					ActivityService.ShowNotification(new("Not registered", "Please register using the external provider first, and then try logging in again"));
					SystemBrowser.OpenBrowser(requiresRegistration.RedirectUri.AbsoluteUri);
					break;

				default:
					ActivityService.ShowErrorNotification($"Unexpected login result: {result}");
					break;
			}
		}
		catch (Exception exception)
		{
			ActivityService.ShowErrorNotification(exception);
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

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_configurationChangeScope?.Dispose();
		}
	}

	private void OnUserLoggedIn()
	{
		UserLoggedIn?.Invoke(this, EventArgs.Empty);
	}
}
