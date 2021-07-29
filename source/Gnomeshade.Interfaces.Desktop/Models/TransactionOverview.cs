// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.Desktop.ViewModels.Binding;

namespace Gnomeshade.Interfaces.Desktop.Models
{
	public sealed class TransactionOverview : PropertyChangedBase
	{
		private DateTime _date;
		private string? _description;
		private string _sourceAccount;
		private string _targetAccount;
		private decimal _sourceAmount;
		private decimal _targetAmount;
		private bool _selected;
		private Guid _id;

		/// <summary>
		/// Gets or sets the id of the transaction which this overview represents.
		/// </summary>
		public Guid Id
		{
			get => _id;
			set => SetAndNotify(ref _id, value, nameof(Id));
		}

		/// <summary>
		/// Gets or sets a value indicating whether this transaction is selected.
		/// </summary>
		public bool Selected
		{
			get => _selected;
			set => SetAndNotify(ref _selected, value, nameof(Selected));
		}

		public DateTime Date
		{
			get => _date;
			set => SetAndNotify(ref _date, value, nameof(Date));
		}

		public string? Description
		{
			get => _description;
			set => SetAndNotify(ref _description, value, nameof(Description));
		}

		public string SourceAccount
		{
			get => _sourceAccount;
			set => SetAndNotify(ref _sourceAccount, value, nameof(SourceAccount));
		}

		public string TargetAccount
		{
			get => _targetAccount;
			set => SetAndNotify(ref _targetAccount, value, nameof(TargetAccount));
		}

		public decimal SourceAmount
		{
			get => _sourceAmount;
			set => SetAndNotify(ref _sourceAmount, value, nameof(SourceAmount));
		}

		public decimal TargetAmount
		{
			get => _targetAmount;
			set => SetAndNotify(ref _targetAmount, value, nameof(TargetAmount));
		}
	}
}
