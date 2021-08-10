// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Interfaces.Desktop.ViewModels.Binding;

namespace Gnomeshade.Interfaces.Desktop.Models
{
	public sealed class TransactionItemOverviewRow : PropertyChangedBase
	{
		private decimal _sourceAmount;
		private decimal _targetAmount;
		private string _product = string.Empty;
		private decimal _amount;
		private string? _description;

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

		public string Product
		{
			get => _product;
			set => SetAndNotify(ref _product, value, nameof(Product));
		}

		public decimal Amount
		{
			get => _amount;
			set => SetAndNotify(ref _amount, value, nameof(Amount));
		}

		public string? Description
		{
			get => _description;
			set => SetAndNotify(ref _description, value, nameof(Description));
		}
	}
}
