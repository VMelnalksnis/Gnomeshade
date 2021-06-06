// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.
// Modified version of https://github.com/Caliburn-Micro/Caliburn.Micro/blob/master/samples/scenarios/Scenario.KeyBinding/Input/KeySequence.cs
// Original Copyright (c) 2010 Blue Spire Consulting, Inc.
// Originally licensed under The MIT License.

using System;
using System.Text;
using System.Windows.Input;

namespace Tracking.Finance.Interfaces.WindowsDesktop.Input
{
	/// <summary>
	/// Class used to store multi-key gesture data.
	/// </summary>
	public sealed class KeySequence
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="KeySequence" /> class.
		/// </summary>
		public KeySequence(ModifierKeys modifiers, params Key[] keys)
		{
			if (keys is null)
			{
				throw new ArgumentNullException(nameof(keys));
			}

			if (keys.Length < 1)
			{
				throw new ArgumentException(@"At least 1 key should be provided", nameof(keys));
			}

			Keys = new Key[keys.Length];
			keys.CopyTo(Keys, 0);
			Modifiers = modifiers;
		}

		/// <summary>
		/// Gets the sequence of keys.
		/// </summary>
		public Key[] Keys { get; }

		/// <summary>
		/// Gets the modifiers to be applied to the sequence.
		/// </summary>
		public ModifierKeys Modifiers { get; }

		/// <summary>
		/// Returns a <see cref="string" /> that represents the current <see cref="object" />.
		/// </summary>
		///
		/// <returns>A <see cref="string" /> that represents the current <see cref="object" />.</returns>
		public sealed override string ToString()
		{
			var builder = new StringBuilder();
			if (Modifiers != ModifierKeys.None)
			{
				if ((Modifiers & ModifierKeys.Control) != ModifierKeys.None)
				{
					_ = builder.Append("Ctrl+");
				}

				if ((Modifiers & ModifierKeys.Alt) != ModifierKeys.None)
				{
					_ = builder.Append("Alt+");
				}

				if ((Modifiers & ModifierKeys.Shift) != ModifierKeys.None)
				{
					_ = builder.Append("Shift+");
				}

				if ((Modifiers & ModifierKeys.Windows) != ModifierKeys.None)
				{
					_ = builder.Append("Windows+");
				}
			}

			_ = builder.Append(Keys[0]);

			foreach (var key in Keys)
			{
				_ = builder.Append($"+{key}");
			}

			return builder.ToString();
		}
	}
}
