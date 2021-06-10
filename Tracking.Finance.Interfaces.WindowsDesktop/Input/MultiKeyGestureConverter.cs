// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

// Modified version of https://github.com/Caliburn-Micro/Caliburn.Micro/blob/master/samples/scenarios/Scenario.KeyBinding/Input/MultiKeyGestureConverter.cs
// Original Copyright (c) 2010 Blue Spire Consulting, Inc.
// Originally licensed under The MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Input;

namespace Tracking.Finance.Interfaces.WindowsDesktop.Input
{
	/// <summary>
	/// Class used to define a converter for the <see cref="MultiKeyGesture" /> class.
	/// </summary>
	///
	/// <remarks>
	/// At the moment it is able to convert strings like <c>Alt+K,R</c> in proper multi-key gestures.
	/// </remarks>
	public sealed class MultiKeyGestureConverter : TypeConverter
	{
		/// <summary>
		/// The default instance of the converter.
		/// </summary>
		public static readonly MultiKeyGestureConverter DefaultConverter = new();

		/// <summary>
		/// The inner key converter.
		/// </summary>
		private static readonly KeyConverter _keyConverter = new();

		/// <summary>
		/// The inner modifier key converter.
		/// </summary>
		private static readonly ModifierKeysConverter _modifierKeysConverter = new();

		/// <summary>
		/// Returns whether this converter can convert an object of the given type to the type of this converter,
		/// using the specified context.
		/// </summary>
		///
		/// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format context.</param>
		/// <param name="sourceType">A <see cref="Type" /> that represents the type you want to convert from.</param>
		/// <returns><see langword="true"/> if this converter can perform the conversion;
		/// otherwise, <see langword="false"/>.</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string);

		/// <summary>
		/// Converts the given object to the type of this converter, using the specified context and culture information.
		/// </summary>
		///
		/// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format context.</param>
		/// <param name="culture">The <see cref="CultureInfo" /> to use as the current culture.</param>
		/// <param name="value">The <see cref="object" /> to convert.</param>
		/// <returns>An <see cref="object" /> that represents the converted value.</returns>
		/// <exception cref="NotSupportedException">The conversion cannot be performed.</exception>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var stringValue = value as string;
			if (string.IsNullOrEmpty(stringValue))
			{
				throw GetConvertFromException(value);
			}

			var sequences = stringValue.Split(',');
			var keySequences = new List<KeySequence>();

			foreach (var sequence in sequences)
			{
				var modifier = ModifierKeys.None;
				var keys = new List<Key>();
				var keyStrings = sequence.Split('+');
				var modifiersCount = 0;

				string temp;
				while ((temp = keyStrings[modifiersCount]) is not null &&
					TryGetModifierKeys(temp.Trim(), out var currentModifier))
				{
					modifiersCount++;
					modifier |= currentModifier;
				}

				for (var i = modifiersCount; i < keyStrings.Length; i++)
				{
					var keyString = keyStrings[i];
					var key = (Key)_keyConverter.ConvertFrom(keyString.Trim());
					keys.Add(key);
				}

				keySequences.Add(new(modifier, keys.ToArray()));
			}

			return new MultiKeyGesture(stringValue, keySequences.ToArray());
		}

		/// <summary>
		/// Converts the given value object to the specified type, using the specified context and culture information.
		/// </summary>
		///
		/// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format context.</param>
		/// <param name="culture">A <see cref="CultureInfo" /> . If null is passed, the current culture is assumed.</param>
		/// <param name="value">The <see cref="object" /> to convert.</param>
		/// <param name="destinationType">The <see cref="Type" /> to convert the <paramref name="value" /> parameter to.</param>
		/// <returns>An <see cref="object" /> that represents the converted value.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="destinationType" /> is <see langword="null"/>.</exception>
		/// <exception cref="NotSupportedException">The conversion cannot be performed.</exception>
		public override object ConvertTo(
			ITypeDescriptorContext context,
			CultureInfo culture,
			object value,
			Type destinationType)
		{
			if (destinationType != typeof(string) || value is not MultiKeyGesture gesture)
			{
				throw GetConvertToException(value, destinationType);
			}

			var builder = new StringBuilder();

			for (var i = 0; i < gesture.KeySequences.Length; i++)
			{
				if (i > 0)
				{
					_ = builder.Append(", ");
				}

				var sequence = gesture.KeySequences[i];
				if (sequence.Modifiers != ModifierKeys.None)
				{
					_ = builder.Append((string)_modifierKeysConverter.ConvertTo(context, culture, sequence.Modifiers, destinationType)!);
					_ = builder.Append('+');
				}

				_ = builder.Append((string)_keyConverter.ConvertTo(context, culture, sequence.Keys[0], destinationType));

				for (var j = 1; j < sequence.Keys.Length; j++)
				{
					_ = builder.Append('+');
					_ = builder.Append((string)_keyConverter.ConvertTo(context, culture, sequence.Keys[0], destinationType));
				}
			}

			return builder.ToString();
		}

		/// <summary>
		/// Tries to get the modifier equivalent to the specified string.
		/// </summary>
		///
		/// <param name="value">The string.</param>
		/// <param name="modifier">The modifier.</param>
		/// <returns><see langword="true"/> if a valid modifier was found; otherwise, <see langword="false"/>.</returns>
		private static bool TryGetModifierKeys(string value, out ModifierKeys modifier)
		{
			var comparer = StringComparer.OrdinalIgnoreCase;
			if (comparer.Equals("CONTROL", value) || comparer.Equals("CTRL", value))
			{
				modifier = ModifierKeys.Control;
				return true;
			}

			if (comparer.Equals("SHIFT", value))
			{
				modifier = ModifierKeys.Shift;
				return true;
			}

			if (comparer.Equals("ALT", value))
			{
				modifier = ModifierKeys.Alt;
				return true;
			}

			if (comparer.Equals("WINDOWS", value) || comparer.Equals("WIN", value))
			{
				modifier = ModifierKeys.Windows;
				return true;
			}

			modifier = ModifierKeys.None;
			return false;
		}
	}
}
