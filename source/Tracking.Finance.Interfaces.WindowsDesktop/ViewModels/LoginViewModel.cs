// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Caliburn.Micro;

using JetBrains.Annotations;

using Tracking.Finance.Interfaces.WebApi.Client;
using Tracking.Finance.Interfaces.WebApi.V1_0.Authentication;
using Tracking.Finance.Interfaces.WindowsDesktop.Events;
using Tracking.Finance.Interfaces.WindowsDesktop.Models;
using Tracking.Finance.Interfaces.WindowsDesktop.Views;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	/// <summary>
	/// Log in form for authenticating the user at startup.
	/// </summary>
	[UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
	public sealed class LoginViewModel : Screen, IViewModel
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly IFinanceClient _financeClient;
		private readonly LoggedInUserModel _loggedInUser;

		private string? _userName;
		private string _password;
		private string? _errorMessage;

		/// <summary>
		/// Initializes a new instance of the <see cref="LoginViewModel"/> class.
		/// </summary>
		/// <param name="eventAggregator">Event aggregator for publishing events.</param>
		/// <param name="financeClient">API client for authentication the user.</param>
		/// <param name="loggedInUser">The user info model which to modify after successful log in.</param>
		public LoginViewModel(
			IEventAggregator eventAggregator,
			IFinanceClient financeClient,
			LoggedInUserModel loggedInUser)
		{
			_eventAggregator = eventAggregator;
			_financeClient = financeClient;
			_loggedInUser = loggedInUser;
		}

		/// <summary>
		/// Gets or sets the user name to use for authentication.
		/// </summary>
		public string? UserName
		{
			get => _userName;
			set
			{
				_userName = value;
				NotifyOfPropertyChange(nameof(UserName));
				NotifyOfPropertyChange(nameof(CanLogIn));
			}
		}

		/// <summary>
		/// Gets or sets the password to use for authentication.
		/// </summary>
		// todo do not bind password
		public string Password
		{
			get => _password;
			set
			{
				_password = value;
				NotifyOfPropertyChange(nameof(Password));
				NotifyOfPropertyChange(nameof(CanLogIn));
			}
		}

		/// <summary>
		/// Gets a value indicating whether the login error message should be visible.
		/// </summary>
		public bool IsErrorVisible => !string.IsNullOrWhiteSpace(ErrorMessage);

		/// <summary>
		/// Gets or sets the login error message.
		/// </summary>
		public string? ErrorMessage
		{
			get => _errorMessage;
			set
			{
				_errorMessage = value;
				NotifyOfPropertyChange(nameof(ErrorMessage));
				NotifyOfPropertyChange(nameof(IsErrorVisible));
			}
		}

		/// <summary>
		/// Gets a value indicating whether <see cref="LogIn"/> can be called.
		/// </summary>
		public bool CanLogIn => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);

		/// <summary>
		/// Authenticates the user using <see cref="UserName"/> and <see cref="Password"/>.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task LogIn()
		{
			try
			{
				ErrorMessage = string.Empty;
				var login = new LoginModel { Username = UserName!, Password = Password };
				_ = await _financeClient.Login(login);
				var userInfo = await _financeClient.Info();

				_loggedInUser.Id = userInfo.Id;
				_loggedInUser.Email = userInfo.Email;
				_loggedInUser.UserName = userInfo.UserName;

				await _eventAggregator.PublishOnUIThreadAsync(new LogOnEvent());
			}
			catch (Exception exception)
			{
				ErrorMessage = exception.Message;
			}
		}

		/// <summary>
		/// Handles Enter key press in the password input.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task PasswordEnterPressed()
		{
			if (CanLogIn)
			{
				await LogIn();
			}
		}

		/// <inheritdoc />
		protected override void OnViewLoaded(object view)
		{
			_ = ((LoginView)view).UserName.Focus();
			base.OnViewLoaded(view);
		}
	}
}
