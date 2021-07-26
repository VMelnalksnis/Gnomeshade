// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Interfaces.Desktop.ViewModels.Binding;

namespace Gnomeshade.Interfaces.Desktop.Models
{
	public sealed class TransactionItem : PropertyChangedBase
	{
		private decimal? _sourceAmount;
		private string? _sourceAccount;
		private decimal? _targetAmount;
		private string? _targetAccount;
		private string? _product;
		private decimal? _amount;

		public decimal? SourceAmount
		{
			get => _sourceAmount;
			set => SetAndNotify(ref _sourceAmount, value, nameof(SourceAmount));
		}

		public string? SourceAccount
		{
			get => _sourceAccount;
			set => SetAndNotify(ref _sourceAccount, value, nameof(SourceAccount));
		}

		public decimal? TargetAmount
		{
			get => _targetAmount;
			set => SetAndNotify(ref _targetAmount, value, nameof(TargetAmount));
		}

		public string? TargetAccount
		{
			get => _targetAccount;
			set => SetAndNotify(ref _targetAccount, value, nameof(TargetAccount));
		}

		public string? Product
		{
			get => _product;
			set => SetAndNotify(ref _product, value, nameof(Product));
		}

		public decimal? Amount
		{
			get => _amount;
			set => SetAndNotify(ref _amount, value, nameof(Amount));
		}
	}
}
