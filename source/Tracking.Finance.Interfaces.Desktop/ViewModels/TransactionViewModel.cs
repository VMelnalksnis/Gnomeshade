namespace Tracking.Finance.Interfaces.Desktop.ViewModels
{
	public sealed class TransactionViewModel : ViewModelBase
	{
		private readonly MainWindowViewModel _mainWindow;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionViewModel"/> class.
		/// </summary>
		public TransactionViewModel()
			: this(new())
		{
		}

		public TransactionViewModel(MainWindowViewModel mainWindow)
		{
			_mainWindow = mainWindow;
		}
	}
}
