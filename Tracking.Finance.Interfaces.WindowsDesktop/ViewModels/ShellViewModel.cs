// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

using Caliburn.Micro;

using Tracking.Finance.Interfaces.WindowsDesktop.Events;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	public sealed class ShellViewModel :
		Conductor<IViewModel>,
		IHandle<LogOnEvent>,
		IHandle<TransactionCreatedEvent>,
		IViewModel
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly SimpleContainer _container;

		/// <summary>
		/// Initializes a new instance of the <see cref="ShellViewModel"/> class.
		/// </summary>
		///
		/// <param name="eventAggregator"></param>
		/// <param name="container"></param>
		public ShellViewModel(
			IEventAggregator eventAggregator,
			SimpleContainer container)
		{
			_eventAggregator = eventAggregator;
			_container = container;

			_eventAggregator.SubscribeOnPublishedThread(this);
			ActivateItemAsync(_container.GetInstance<LoginViewModel>()).GetAwaiter().GetResult();
		}

		/// <inheritdoc/>
		public async Task HandleAsync(LogOnEvent message, CancellationToken cancellationToken)
		{
			var transactions = _container.GetInstance<TransactionViewModel>();
			await ActivateItemAsync(transactions, cancellationToken);
		}

		/// <inheritdoc/>
		public async Task HandleAsync(TransactionCreatedEvent message, CancellationToken cancellationToken)
		{
			// todo transaction id?
			var transaction = _container.GetInstance<TransactionViewModel>();
			await ActivateItemAsync(transaction, cancellationToken);
		}
	}
}
