// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.Desktop.ViewModels.Binding
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
		/// <param name="guardPropertyNames">The names of the computed guard properties.</param>
		/// <typeparam name="T">The type of the property.</typeparam>
		[NotifyPropertyChangedInvocator]
		protected void SetAndNotifyWithGuard<T>(
			ref T backingField,
			T newValue,
			[CallerMemberName] string propertyName = "",
			params string[] guardPropertyNames)
		{
			if (EqualityComparer<T>.Default.Equals(backingField, newValue))
			{
				return;
			}

			OnPropertiesChanging(propertyName, guardPropertyNames);
			backingField = newValue;
			OnPropertiesChanged(propertyName, guardPropertyNames);
		}

		private void OnPropertyChanging([CallerMemberName] string propertyName = "")
		{
			PropertyChanging?.Invoke(this, new(propertyName));
		}

		private void OnPropertiesChanging(string property, params string[] propertyNames)
		{
			if (PropertyChanging is null)
			{
				return;
			}

			PropertyChanging(this, new(property));
			foreach (var propertyName in propertyNames)
			{
				PropertyChanging(this, new(propertyName));
			}
		}

		[NotifyPropertyChangedInvocator]
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new(propertyName));
		}

		private void OnPropertiesChanged(string property, params string[] propertyNames)
		{
			if (PropertyChanged is null)
			{
				return;
			}

			PropertyChanged(this, new(property));
			foreach (var propertyName in propertyNames)
			{
				PropertyChanged(this, new(propertyName));
			}
		}
	}
}
