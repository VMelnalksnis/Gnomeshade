// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

// Modified version of https://github.com/Caliburn-Micro/Caliburn.Micro/blob/master/samples/scenarios/Scenario.KeyBinding/Input/KeyTrigger.cs
// Original Copyright (c) 2010 Blue Spire Consulting, Inc.
// Originally licensed under The MIT License.

using System.Windows;
using System.Windows.Input;

using JetBrains.Annotations;

using Microsoft.Xaml.Behaviors;

namespace Tracking.Finance.Interfaces.WindowsDesktop.Input
{
	public sealed class KeyTrigger : TriggerBase<UIElement>
	{
		public static readonly DependencyProperty KeyProperty =
			DependencyProperty.Register(nameof(Key), typeof(Key), typeof(KeyTrigger), null);

		public static readonly DependencyProperty ModifiersProperty =
			DependencyProperty.Register(nameof(Modifiers), typeof(ModifierKeys), typeof(KeyTrigger), null);

		[UsedImplicitly]
		public Key Key
		{
			get => (Key)GetValue(KeyProperty);
			set => SetValue(KeyProperty, value);
		}

		[UsedImplicitly]
		public ModifierKeys Modifiers
		{
			get => (ModifierKeys)GetValue(ModifiersProperty);
			set => SetValue(ModifiersProperty, value);
		}

		/// <inheritdoc/>
		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.KeyDown += OnAssociatedObjectKeyDown;
		}

		/// <inheritdoc/>
		protected override void OnDetaching()
		{
			base.OnDetaching();
			AssociatedObject.KeyDown -= OnAssociatedObjectKeyDown;
		}

		private static ModifierKeys GetActualModifiers(Key key, ModifierKeys modifiers) => key switch
		{
			Key.LeftCtrl or Key.RightCtrl => modifiers | ModifierKeys.Control,
			Key.LeftAlt or Key.RightAlt => modifiers | ModifierKeys.Alt,
			Key.LeftShift or Key.RightShift => modifiers | ModifierKeys.Shift,
			_ => modifiers,
		};

		private void OnAssociatedObjectKeyDown(object sender, KeyEventArgs keyEvent)
		{
			var key = (keyEvent.Key == Key.System) ? keyEvent.SystemKey : keyEvent.Key;
			if ((key == Key) && (Keyboard.Modifiers == GetActualModifiers(keyEvent.Key, Modifiers)))
			{
				InvokeActions(keyEvent);
			}
		}
	}
}
