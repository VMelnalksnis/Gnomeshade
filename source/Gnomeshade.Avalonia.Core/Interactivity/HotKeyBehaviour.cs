// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace Gnomeshade.Avalonia.Core.Interactivity;

/// <summary>Behaviour that executes a command when a hotkey has been pressed.</summary>
public sealed class HotKeyBehaviour : Trigger<Control>
{
	/// <summary>Identifies the <seealso cref="TopLevel"/> avalonia property.</summary>
	public static readonly DirectProperty<HotKeyBehaviour, TopLevel?> TopLevelProperty =
		AvaloniaProperty.RegisterDirect<HotKeyBehaviour, TopLevel?>(
			nameof(TopLevel),
			behaviour => behaviour.TopLevel,
			(behaviour, topLevel) => behaviour.TopLevel = topLevel);

	/// <summary>Identifies the <seealso cref="HotKey"/> avalonia property.</summary>
	public static readonly DirectProperty<HotKeyBehaviour, KeyGesture?> HotKeyProperty =
		AvaloniaProperty.RegisterDirect<HotKeyBehaviour, KeyGesture?>(
			nameof(HotKey),
			behaviour => behaviour.HotKey,
			(behaviour, keyGesture) => behaviour.HotKey = keyGesture);

	/// <summary>Identifies the <seealso cref="Command"/> avalonia property.</summary>
	public static readonly DirectProperty<HotKeyBehaviour, ICommand?> CommandProperty =
		AvaloniaProperty.RegisterDirect<HotKeyBehaviour, ICommand?>(
			nameof(Command),
			behaviour => behaviour.Command,
			(behaviour, command) => behaviour.Command = command);

	private KeyBinding? _gesture;

	/// <summary>Gets or sets the <see cref="TopLevel"/> control in which to register the <see cref="HotKey"/>.</summary>
	public TopLevel? TopLevel { get; set; }

	/// <summary>Gets or sets the <see cref="KeyGesture"/> which will trigger the <see cref="Command"/>.</summary>
	public KeyGesture? HotKey { get; set; }

	/// <summary>Gets or sets the <see cref="ICommand"/> that will be executed when <see cref="HotKey"/> is pressed.</summary>
	public ICommand? Command { get; set; }

	/// <inheritdoc />
	protected override void OnAttachedToVisualTree()
	{
		if (TopLevel is { } topLevel && Command is { } command && HotKey is { } hotKey)
		{
			_gesture = new() { Command = command, Gesture = hotKey };
			topLevel.KeyBindings.Add(_gesture);
		}
	}

	/// <inheritdoc />
	protected override void OnDetachedFromVisualTree()
	{
		if (TopLevel is { } topLevel && _gesture is { } gesture)
		{
			topLevel.KeyBindings.Remove(gesture);
		}
	}
}
