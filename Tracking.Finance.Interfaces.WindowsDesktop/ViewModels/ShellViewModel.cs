using Caliburn.Micro;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	public sealed class ShellViewModel : Conductor<object>
	{
		private LoginViewModel _loginViewModel;

		public ShellViewModel(LoginViewModel loginViewModel)
		{
			_loginViewModel = loginViewModel;
			ActivateItemAsync(_loginViewModel).GetAwaiter().GetResult();
		}
	}
}
