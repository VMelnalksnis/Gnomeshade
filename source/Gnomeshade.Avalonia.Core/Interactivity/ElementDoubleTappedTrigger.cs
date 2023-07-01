// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace Gnomeshade.Avalonia.Core.Interactivity;

/// <summary>Triggers actions on <see cref="InputElement.DoubleTapped"/> event.</summary>
public sealed class ElementDoubleTappedTrigger : Trigger<InputElement>
{
	/// <inheritdoc />
	protected override void OnAttachedToVisualTree()
	{
		if (AssociatedObject is { } inputElement)
		{
			inputElement.DoubleTapped += InputElementOnDoubleTapped;
		}
	}

	/// <inheritdoc />
	protected override void OnDetachedFromVisualTree()
	{
		if (AssociatedObject is { } inputElement)
		{
			inputElement.DoubleTapped -= InputElementOnDoubleTapped;
		}
	}

	private void InputElementOnDoubleTapped(object? sender, TappedEventArgs eventArgs)
	{
		Interaction.ExecuteActions(this, Actions, eventArgs);
	}
}
