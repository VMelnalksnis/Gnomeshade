﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

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
