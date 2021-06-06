// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Caliburn.Micro;

namespace Tracking.Finance.Interfaces.WindowsDesktop.Models
{
	public sealed class TransactionItemModel : PropertyChangedBase
	{
		private string _targetAccount;
		private string _sourceAccount;
		private decimal _sourceAmount;
		private decimal _targetAmount;
		private string _product;
		private uint _quantity;
		private DateTime? _deliveryDate;
		private string? _bankReference;
		private string? _externalReference;
		private string? _internalReference;

		public string SourceAccount
		{
			get => _sourceAccount;
			set
			{
				_sourceAccount = value;
				NotifyOfPropertyChange(() => SourceAccount);
			}
		}

		public string TargetAccount
		{
			get => _targetAccount;
			set
			{
				_targetAccount = value;
				NotifyOfPropertyChange(() => TargetAccount);
			}
		}

		public decimal SourceAmount
		{
			get => _sourceAmount;
			set
			{
				_sourceAmount = value;
				NotifyOfPropertyChange(() => SourceAmount);
			}
		}

		public decimal TargetAmount
		{
			get => _targetAmount;
			set
			{
				_targetAmount = value;
				NotifyOfPropertyChange(() => TargetAmount);
			}
		}

		public string Product
		{
			get => _product;
			set
			{
				_product = value;
				NotifyOfPropertyChange(() => Product);
			}
		}

		public uint Quantity
		{
			get => _quantity;
			set
			{
				_quantity = value;
				NotifyOfPropertyChange(() => Quantity);
			}
		}

		public DateTime? DeliveryDate
		{
			get => _deliveryDate;
			set
			{
				_deliveryDate = value;
				NotifyOfPropertyChange(() => DeliveryDate);
			}
		}

		public string? BankReference
		{
			get => _bankReference;
			set
			{
				_bankReference = value;
				NotifyOfPropertyChange(() => BankReference);
			}
		}

		public string? ExternalReference
		{
			get => _externalReference;
			set
			{
				_externalReference = value;
				NotifyOfPropertyChange(() => ExternalReference);
			}
		}

		public string? InternalReference
		{
			get => _internalReference;
			set
			{
				_internalReference = value;
				NotifyOfPropertyChange(() => InternalReference);
			}
		}
	}
}
