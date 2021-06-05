﻿using System;
using System.Threading.Tasks;

using Caliburn.Micro;

using Tracking.Finance.Interfaces.WebApi.Client;
using Tracking.Finance.Interfaces.WebApi.v1_0.Authentication;
using Tracking.Finance.Interfaces.WindowsDesktop.Models;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	public sealed class LoginViewModel : Screen
	{
		private readonly IFinanceClient _financeClient;
		private readonly LoggedInUserModel _loggedInUser;

		private string _userName;
		private string _password;
		private string _errorMessage;

		public LoginViewModel(IFinanceClient financeClient, LoggedInUserModel loggedInUser)
		{
			_financeClient = financeClient;
			_loggedInUser = loggedInUser;
		}

		public string UserName
		{
			get => _userName;
			set
			{
				_userName = value;
				NotifyOfPropertyChange(() => UserName);
				NotifyOfPropertyChange(() => CanLogIn);
			}
		}

		public string Password
		{
			get => _password;
			set
			{
				_password = value;
				NotifyOfPropertyChange(() => Password);
				NotifyOfPropertyChange(() => CanLogIn);
			}
		}

		public bool IsErrorVisible => !string.IsNullOrWhiteSpace(ErrorMessage);

		public string ErrorMessage
		{
			get => _errorMessage;
			set
			{
				_errorMessage = value;
				NotifyOfPropertyChange(() => ErrorMessage);
				NotifyOfPropertyChange(() => IsErrorVisible);
			}
		}

		public bool CanLogIn => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);

		public async Task LogIn()
		{
			try
			{
				ErrorMessage = string.Empty;
				var login = new LoginModel { Username = UserName, Password = Password };
				var loginResponse = await _financeClient.Login(login);
				var userInfo = await _financeClient.Info();

				_loggedInUser.Id = userInfo.Id;
				_loggedInUser.Email = userInfo.Email;
				_loggedInUser.UserName = userInfo.UserName;
			}
			catch (Exception exception)
			{
				ErrorMessage = exception.Message;
			}
		}
	}
}
