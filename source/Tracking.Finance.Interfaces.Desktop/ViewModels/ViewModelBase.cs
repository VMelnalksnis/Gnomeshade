// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

namespace Tracking.Finance.Interfaces.Desktop.ViewModels
{
	public abstract class ViewModelBase : INotifyPropertyChanged
	{
		/// <inheritdoc />
		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new(propertyName));
		}

		[NotifyPropertyChangedInvocator]
		protected void OnPropertiesChanged([CallerMemberName] string property1 = "", string property2 = "")
		{
			if (PropertyChanged is null)
			{
				return;
			}

			PropertyChanged(this, new(property1));
			PropertyChanged(this, new(property2));
		}
	}
}
