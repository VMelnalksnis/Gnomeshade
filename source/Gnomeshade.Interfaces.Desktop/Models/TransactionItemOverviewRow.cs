// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.Desktop.ViewModels.Binding;

namespace Gnomeshade.Interfaces.Desktop.Models
{
	public sealed class TransactionItemOverviewRow : PropertyChangedBase
	{
		private readonly Guid _id;
		private readonly decimal _sourceAmount;
		private readonly decimal _targetAmount;
		private readonly string _product = string.Empty;
		private readonly decimal _amount;
		private readonly string? _description;

		/// <summary>
		/// Gets the id of the transaction item.
		/// </summary>
		public Guid Id
		{
			get => _id;
			init => SetAndNotify(ref _id, value, nameof(Id));
		}

		public decimal SourceAmount
		{
			get => _sourceAmount;
			init => SetAndNotify(ref _sourceAmount, value, nameof(SourceAmount));
		}

		public decimal TargetAmount
		{
			get => _targetAmount;
			init => SetAndNotify(ref _targetAmount, value, nameof(TargetAmount));
		}

		public string Product
		{
			get => _product;
			init => SetAndNotify(ref _product, value, nameof(Product));
		}

		public decimal Amount
		{
			get => _amount;
			init => SetAndNotify(ref _amount, value, nameof(Amount));
		}

		public string? Description
		{
			get => _description;
			init => SetAndNotify(ref _description, value, nameof(Description));
		}
	}
}
