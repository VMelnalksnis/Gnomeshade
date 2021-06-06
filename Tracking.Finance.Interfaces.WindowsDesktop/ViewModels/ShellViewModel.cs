using System.Threading;
using System.Threading.Tasks;

using Caliburn.Micro;

using Tracking.Finance.Interfaces.WindowsDesktop.Events;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	public sealed class ShellViewModel : Conductor<object>, IHandle<LogOnEvent>, IViewModel
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly TransactionViewModel _transactionViewModel;
		private readonly SimpleContainer _container;

		public ShellViewModel(
			IEventAggregator eventAggregator,
			TransactionViewModel transactionViewModel,
			SimpleContainer container)
		{
			_eventAggregator = eventAggregator;
			_transactionViewModel = transactionViewModel;
			_container = container;

			_eventAggregator.SubscribeOnPublishedThread(this);
			ActivateItemAsync(_container.GetInstance<LoginViewModel>()).GetAwaiter().GetResult();
		}

		/// <inheritdoc/>
		public async Task HandleAsync(LogOnEvent message, CancellationToken cancellationToken)
		{
			await ActivateItemAsync(_transactionViewModel, cancellationToken);
		}
	}
}
