using System.Windows;
using System.Windows.Input;

using Microsoft.Xaml.Behaviors;

namespace Tracking.Finance.Interfaces.WindowsDesktop.Input
{
	public sealed class KeyTrigger : TriggerBase<UIElement>
	{
		public static readonly DependencyProperty KeyProperty =
			DependencyProperty.Register(nameof(Key), typeof(Key), typeof(KeyTrigger), null);

		public static readonly DependencyProperty ModifiersProperty =
			DependencyProperty.Register(nameof(Modifiers), typeof(ModifierKeys), typeof(KeyTrigger), null);

		public Key Key
		{
			get => (Key)GetValue(KeyProperty);
			set => SetValue(KeyProperty, value);
		}

		public ModifierKeys Modifiers
		{
			get => (ModifierKeys)GetValue(ModifiersProperty);
			set => SetValue(ModifiersProperty, value);
		}

		/// <inheritdoc/>
		protected sealed override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.KeyDown += OnAssociatedObjectKeyDown;
		}

		/// <inheritdoc/>
		protected sealed override void OnDetaching()
		{
			base.OnDetaching();
			AssociatedObject.KeyDown -= OnAssociatedObjectKeyDown;
		}

		private static ModifierKeys GetActualModifiers(Key key, ModifierKeys modifiers) => key switch
		{
			Key.LeftCtrl or Key.RightCtrl => modifiers |= ModifierKeys.Control,
			Key.LeftAlt or Key.RightAlt => modifiers |= ModifierKeys.Alt,
			Key.LeftShift or Key.RightShift => modifiers |= ModifierKeys.Shift,
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
