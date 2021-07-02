using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Tracking.Finance.Interfaces.Desktop.Views
{
	public class TransactionView : UserControl
	{
		public TransactionView()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}

