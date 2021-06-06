// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Caliburn.Micro;

using Tracking.Finance.Interfaces.WebApi.Client;
using Tracking.Finance.Interfaces.WebApi.V1_0.Transactions;

using TransactionItemModel = Tracking.Finance.Interfaces.WindowsDesktop.Models.TransactionItemModel;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	public sealed class TransactionViewModel : Screen, IViewModel
	{
		private readonly IFinanceClient _financeClient;

		private DateTime? _date = DateTime.Now;
		private string? _description;
		private ObservableCollection<TransactionItemModel> _transactionItems = new()
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

		public TransactionViewModel(IFinanceClient financeClient)
		{
			_financeClient = financeClient;
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

		public ObservableCollection<TransactionItemModel> TransactionItems
		{
			get => _transactionItems;
			set
			{
				_transactionItems = value;
				NotifyOfPropertyChange(() => TransactionItems);
			}
		}

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
			var transaction = new TransactionCreationModel
			{
				Description = Description,
				Date = Date.HasValue ? new DateTimeOffset(Date.Value, DateTimeOffset.Now.Offset) : null,
			};

			var transactionId = await _financeClient.Create(transaction);
			var items = TransactionItems
				.Select(item => new TransactionItemCreationModel
				{
					SourceAccountId = 0, // todo
					TargetAccountId = 0, // todo
					SourceAmount = item.SourceAmount,
					TargetAmount = item.TargetAmount,
					BankReference = item.BankReference,
					ExternalReference = item.ExternalReference,
					InternalReference = item.InternalReference,
					ProductId = 0, // todo
					Amount = item.Quantity,
				})
				.ToList();

			foreach (var item in items)
			{
				_ = await _financeClient.CreateItem(transactionId, item);
			}
		}

		public void CtrlNPressed() => AddItem();
	}
}
