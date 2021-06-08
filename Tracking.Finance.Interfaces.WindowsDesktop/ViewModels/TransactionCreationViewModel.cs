// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

using Caliburn.Micro;

using Tracking.Finance.Interfaces.WebApi.Client;
using Tracking.Finance.Interfaces.WebApi.V1_0.Transactions;
using Tracking.Finance.Interfaces.WindowsDesktop.Events;
using Tracking.Finance.Interfaces.WindowsDesktop.Helpers;

using TransactionItemModel = Tracking.Finance.Interfaces.WindowsDesktop.Models.TransactionItemModel;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	public sealed class TransactionCreationViewModel : Screen, IViewModel
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly IFinanceClient _financeClient;

		private DateTime? _date = DateTime.Now;
		private string? _description;
		private ObervableItemCollection<TransactionItemModel> _transactionItems;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionCreationViewModel"/> class.
		/// </summary>
		public TransactionCreationViewModel(
			IEventAggregator eventAggregator,
			IFinanceClient financeClient)
		{
			_eventAggregator = eventAggregator;
			_financeClient = financeClient;

			TransactionItems = new()
			{
				new TransactionItemModel
				{
					SourceAccount = "Wallet",
					TargetAccount = "Rimi",
					SourceAmount = 1.05m,
					TargetAmount = 1.05m,
					Product = "Bread",
					Quantity = 1,
					DeliveryDate = DateTime.Now,
				},
			};
		}

		public DateTime? Date
		{
			get => _date;
			set
			{
				_date = value;
				NotifyOfPropertyChange(() => Date);
			}
		}

		public string? Description
		{
			get => _description;
			set
			{
				_description = value;
				NotifyOfPropertyChange(() => Description);
			}
		}

		public ObervableItemCollection<TransactionItemModel> TransactionItems
		{
			get => _transactionItems;
			set
			{
				if (_transactionItems is not null)
				{
					_transactionItems.CollectionChanged -= TransactionItems_CollectionChanged;
				}

				_transactionItems = value;
				_transactionItems.CollectionChanged += TransactionItems_CollectionChanged;
				NotifyOfPropertyChange(() => TransactionItems);
			}
		}

		public decimal TotalSourceAmount => TransactionItems.Sum(item => item.SourceAmount);

		public decimal TotalTargetAmount => TransactionItems.Sum(item => item.TargetAmount);

		public bool CanAddItem => true;

		public bool CanSave => TransactionItems.Any();

		public void AddItem()
		{
			if (!TransactionItems.Any())
			{
				TransactionItems.Add(new TransactionItemModel());
				return;
			}

			var lastItem = TransactionItems[^1];
			var newItem = new TransactionItemModel
			{
				SourceAccount = lastItem.SourceAccount,
				TargetAccount = lastItem.TargetAccount,
				DeliveryDate = lastItem.DeliveryDate,
				BankReference = lastItem.BankReference,
				ExternalReference = lastItem.ExternalReference,
				InternalReference = lastItem.InternalReference,
			};

			TransactionItems.Add(newItem);
		}

		public async Task Save()
		{
			var items = TransactionItems
				.Select(item => new TransactionItemCreationModel
				{
					SourceAccountId = Guid.Empty, // todo
					TargetAccountId = Guid.Empty, // todo
					SourceAmount = item.SourceAmount,
					TargetAmount = item.TargetAmount,
					BankReference = item.BankReference,
					ExternalReference = item.ExternalReference,
					InternalReference = item.InternalReference,
					ProductId = Guid.Empty, // todo
					Amount = item.Quantity,
				})
				.ToList();

			var transaction = new TransactionCreationModel
			{
				Description = Description,
				Date = Date.HasValue ? new DateTimeOffset(Date.Value, DateTimeOffset.Now.Offset) : null,
				Items = items,
			};

			var transactionId = await _financeClient.Create(transaction);
			await _eventAggregator.PublishOnUIThreadAsync(new TransactionCreatedEvent(transactionId));
		}

		public void CtrlNPressed() => AddItem();

		private void TransactionItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			NotifyOfPropertyChange(() => TotalSourceAmount);
			NotifyOfPropertyChange(() => TotalTargetAmount);
		}
	}
}
