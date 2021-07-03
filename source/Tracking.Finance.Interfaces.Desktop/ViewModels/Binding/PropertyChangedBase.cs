// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

namespace Tracking.Finance.Interfaces.Desktop.ViewModels.Observable
{
	public abstract class PropertyChangedBase : INotifyPropertyChanging, INotifyPropertyChanged
	{
		/// <inheritdoc />
		public event PropertyChangingEventHandler? PropertyChanging;

		/// <inheritdoc />
		public event PropertyChangedEventHandler? PropertyChanged;

		/// <summary>
		/// Sets the backing field of a property, raises <see cref="PropertyChanging"/> and <see cref="PropertyChanged"/>,
		/// if <paramref name="newValue"/> does not equal <paramref name="backingField"/>.
		/// </summary>
		/// <param name="backingField">A reference to the backing field of a property.</param>
		/// <param name="newValue">The new value to set.</param>
		/// <param name="propertyName">The name of the property being modified.</param>
		/// <typeparam name="T">The type of the property.</typeparam>
		[NotifyPropertyChangedInvocator]
		protected void SetAndNotify<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = "")
		{
			if (EqualityComparer<T>.Default.Equals(backingField, newValue))
			{
				return;
			}

			OnPropertyChanging(propertyName);
			backingField = newValue;
			OnPropertyChanged(propertyName);
		}

		/// <summary>
		/// Sets the backing field of a property, raises <see cref="PropertyChanging"/> and <see cref="PropertyChanged"/>
		/// for the property and the specified computed guard property,
		/// if <paramref name="newValue"/> does not equal <paramref name="backingField"/>.
		/// </summary>
		/// <param name="backingField">A reference to the backing field of a property.</param>
		/// <param name="newValue">The new value to set.</param>
		/// <param name="propertyName">The name of the property being modified.</param>
		/// <param name="guardPropertyName">The name of the computed guard property.</param>
		/// <typeparam name="T">The type of the property.</typeparam>
		[NotifyPropertyChangedInvocator]
		protected void SetAndNotifyWithGuard<T>(
			ref T backingField,
			T newValue,
			[CallerMemberName] string propertyName = "",
			[CallerMemberName] string guardPropertyName = "")
		{
			if (EqualityComparer<T>.Default.Equals(backingField, newValue))
			{
				return;
			}

			OnPropertiesChanging(propertyName, guardPropertyName);
			backingField = newValue;
			OnPropertiesChanged(propertyName, guardPropertyName);
		}

		[NotifyPropertyChangedInvocator]
		private void OnPropertyChanging([CallerMemberName] string propertyName = "")
		{
			PropertyChanging?.Invoke(this, new(propertyName));
		}

		[NotifyPropertyChangedInvocator]
		private void OnPropertiesChanging([CallerMemberName] string property1 = "", string property2 = "")
		{
			if (PropertyChanging is null)
			{
				return;
			}

			PropertyChanging(this, new(property1));
			PropertyChanging(this, new(property2));
		}

		[NotifyPropertyChangedInvocator]
		private void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new(propertyName));
		}

		[NotifyPropertyChangedInvocator]
		private void OnPropertiesChanged([CallerMemberName] string property1 = "", string property2 = "")
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
