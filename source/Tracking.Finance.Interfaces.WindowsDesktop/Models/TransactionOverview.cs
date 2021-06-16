// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Caliburn.Micro;

namespace Tracking.Finance.Interfaces.WindowsDesktop.Models
{
	public sealed class TransactionOverview : PropertyChangedBase
	{
		private DateTime _date;
		private string? _description;
		private string _sourceAccount;
		private string _targetAccount;
		private decimal _sourceAmount;
		private decimal _targetAmount;

		public DateTime Date
		{
			get => _date;
			set
			{
				_date = value;
				NotifyOfPropertyChange(nameof(Date));
			}
		}

		public string? Description
		{
			get => _description;
			set
			{
				_description = value;
				NotifyOfPropertyChange(nameof(Description));
			}
		}

		public string SourceAccount
		{
			get => _sourceAccount;
			set
			{
				_sourceAccount = value;
				NotifyOfPropertyChange(nameof(SourceAccount));
			}
		}

		public string TargetAccount
		{
			get => _targetAccount;
			set
			{
				_targetAccount = value;
				NotifyOfPropertyChange(nameof(TargetAccount));
			}
		}

		public decimal SourceAmount
		{
			get => _sourceAmount;
			set
			{
				_sourceAmount = value;
				NotifyOfPropertyChange(nameof(SourceAmount));
			}
		}

		public decimal TargetAmount
		{
			get => _targetAmount;
			set
			{
				_targetAmount = value;
				NotifyOfPropertyChange(nameof(TargetAmount));
			}
		}
	}
}
