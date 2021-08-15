// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.Desktop.ViewModels.Binding;

namespace Gnomeshade.Interfaces.Desktop.Models
{
	public sealed class ProductOverviewRow : PropertyChangedBase
	{
		private string _name = string.Empty;
		private Guid _id;

		/// <summary>
		/// Gets or sets the id of the product.
		/// </summary>
		public Guid Id
		{
			get => _id;
			set => SetAndNotify(ref _id, value, nameof(Id));
		}

		/// <summary>
		/// Gets or sets the name of the product.
		/// </summary>
		public string Name
		{
			get => _name;
			set => SetAndNotify(ref _name, value, nameof(Name));
		}
	}
}
