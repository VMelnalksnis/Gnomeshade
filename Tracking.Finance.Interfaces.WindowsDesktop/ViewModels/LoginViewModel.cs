using System;

using Caliburn.Micro;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	public sealed class LoginViewModel : Screen
	{
		private string _userName;
		private string _password;

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

		public bool CanLogIn => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);

		public void LogIn()
		{
			Console.WriteLine("Logged in");
		}
	}
}
