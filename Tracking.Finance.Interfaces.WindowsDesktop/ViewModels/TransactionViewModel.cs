using System;
using System.ComponentModel;

using Caliburn.Micro;

namespace Tracking.Finance.Interfaces.WindowsDesktop.ViewModels
{
	public class TransactionViewModel : Screen
	{
		private DateTimeOffset _date = DateTimeOffset.Now;
		private string? _description;
		private BindingList<string> _items;

		public DateTimeOffset Date
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

		public BindingList<string> Items
		{
			get => _items;
			set
			{
				_items = value;
				NotifyOfPropertyChange(() => Items);
			}
		}

		public bool CanAddItem
		{
			get
			{
				// todo validation?
				return true;
			}
		}

		public bool CanSave
		{
			get
			{
				// todo validation
				return false;
			}
		}

		public void AddItem()
		{
		}

		public void Save()
		{
		}
	}
}
