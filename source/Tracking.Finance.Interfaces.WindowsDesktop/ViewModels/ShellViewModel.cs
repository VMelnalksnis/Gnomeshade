// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

using Caliburn.Micro;

using Tracking.Finance.Interfaces.WindowsDesktop.Events;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	/// <summary>
	/// The main viewmodel which managed the currently active <see cref="IViewModel"/>.
	/// </summary>
	public sealed class ShellViewModel :
		Conductor<IViewModel>,
		IHandle<LogOnEvent>,
		IHandle<TransactionCreatedEvent>,
		IHandle<CreateNewTransactionEvent>,
		IViewModel
	{
		private readonly SimpleContainer _container;

		/// <summary>
		/// Initializes a new instance of the <see cref="ShellViewModel"/> class.
		/// </summary>
		/// <param name="eventAggregator">The event aggregator on which to subscribe to events regarding view changes.</param>
		/// <param name="container">Dependency injection container for view model instantiation.</param>
		public ShellViewModel(
			IEventAggregator eventAggregator,
			SimpleContainer container)
		{
			_container = container;

			eventAggregator.SubscribeOnPublishedThread(this);
			Task.Run(async () => await ActivateItemAsync(_container.GetInstance<LoginViewModel>())).Wait();
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

		/// <inheritdoc />
		public async Task HandleAsync(CreateNewTransactionEvent message, CancellationToken cancellationToken)
		{
			var transactionCreation = _container.GetInstance<TransactionCreationViewModel>();
			await ActivateItemAsync(transactionCreation, cancellationToken);
		}
	}
}
