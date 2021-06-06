using System;
using System.Collections.ObjectModel;
using System.Linq;

using Caliburn.Micro;

using Tracking.Finance.Interfaces.WindowsDesktop.Models;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	public sealed class TransactionViewModel : Screen, IViewModel
	{
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

		public void Save()
		{
		}

		public void CtrlNPressed()
		{
			AddItem();
		}
	}
}
