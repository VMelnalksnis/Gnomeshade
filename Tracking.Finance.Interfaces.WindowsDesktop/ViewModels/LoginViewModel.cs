using System;
using System.Threading.Tasks;

using Caliburn.Micro;

using Tracking.Finance.Interfaces.WindowsDesktop.Helpers;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	public sealed class LoginViewModel : Screen
	{
		private readonly IApiClient _apiClient;

		private string _userName;
		private string _password;

		public LoginViewModel(IApiClient apiClient)
		{
			_apiClient = apiClient;
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
				var result = await _apiClient.Authenticate(UserName, Password);
			}
			catch (Exception exception)
			{
				Console.Error.WriteLine(exception.ToString());
			}
		}
	}
}
