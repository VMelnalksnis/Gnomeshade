using System;
using System.Threading.Tasks;

using Caliburn.Micro;

using Tracking.Finance.Interfaces.WebApi.Client;
using Tracking.Finance.Interfaces.WebApi.v1_0.Authentication;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	public sealed class LoginViewModel : Screen
	{
		private readonly IFinanceClient _financeClient;

		private string _userName;
		private string _password;

		public LoginViewModel(IFinanceClient financeClient)
		{
			_financeClient = financeClient;
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

		public bool CanLogIn => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);

		public async Task LogIn()
		{
			try
			{
				var login = new LoginModel { Username = UserName, Password = Password };
				var result = await _financeClient.Login(login);
			}
			catch (Exception exception)
			{
				Console.Error.WriteLine(exception.ToString());
			}
		}
	}
}
