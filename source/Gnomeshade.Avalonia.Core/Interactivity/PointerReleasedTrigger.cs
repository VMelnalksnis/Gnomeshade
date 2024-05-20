// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace Gnomeshade.Avalonia.Core.Interactivity;

/// <summary>Triggers actions based on <see cref="InputElement.PointerReleasedEvent"/> routed event.</summary>
public sealed class PointerReleasedTrigger : Trigger<InputElement>
{
	/// <summary>Identifies the <seealso cref="SourceInteractive"/> avalonia property.</summary>
	public static readonly StyledProperty<Interactive?> SourceInteractiveProperty =
		AvaloniaProperty.Register<PointerReleasedTrigger, Interactive?>(nameof(SourceInteractive));

	/// <summary>Identifies the <seealso cref="MouseButton"/> avalonia property.</summary>
	public static readonly StyledProperty<MouseButton> MouseButtonProperty =
		AvaloniaProperty.Register<PointerReleasedTrigger, MouseButton>(nameof(MouseButton));

	private bool _isInitialized;

	/// <summary>
	/// Gets or sets the source object from which this behavior listens for events.
	/// If <seealso cref="SourceInteractive"/> is not set, the source will default to <seealso cref="Behavior.AssociatedObject"/>. This is an avalonia property.
	/// </summary>
	public Interactive? SourceInteractive
	{
		get => GetValue(SourceInteractiveProperty);
		set => SetValue(SourceInteractiveProperty, value);
	}

	/// <summary>Gets or sets the mouse button for which the trigger will list for.</summary>
	public MouseButton MouseButton
	{
		get => GetValue(MouseButtonProperty);
		set => SetValue(MouseButtonProperty, value);
	}

	private static RoutedEvent<PointerReleasedEventArgs> RoutedEvent => InputElement.PointerReleasedEvent;

	private Interactive? Interactive => SourceInteractive ?? AssociatedObject;

	/// <inheritdoc />
	protected override void OnAttachedToVisualTree()
	{
		if (Interactive is { } interactive)
		{
			interactive.AddHandler(RoutedEvent, Handler, RoutingStrategies.Tunnel);
			_isInitialized = true;
		}
	}

	/// <inheritdoc />
	protected override void OnDetachedFromVisualTree()
	{
		if (Interactive is { } interactive && _isInitialized)
		{
			interactive.RemoveHandler(RoutedEvent, Handler);
			_isInitialized = false;
		}
	}

	private void Handler(object? sender, PointerReleasedEventArgs e)
	{
		if (Interactive is { } interactive && e.InitialPressMouseButton == MouseButton)
		{
			Interaction.ExecuteActions(interactive, Actions, e);
		}
	}
}
