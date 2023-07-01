// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace Gnomeshade.Avalonia.Core.Interactivity;

/// <summary>Triggers actions on <see cref="WindowBase.Activated"/> event.</summary>
public sealed class WindowActivatedTrigger : Trigger<WindowBase>
{
	/// <inheritdoc />
	protected override void OnAttached()
	{
		if (AssociatedObject is { } window)
		{
			window.Activated += WindowOnActivated;
		}
	}

	/// <inheritdoc />
	protected override void OnDetaching()
	{
		if (AssociatedObject is { } window)
		{
			window.Activated -= WindowOnActivated;
		}
	}

	private void WindowOnActivated(object? sender, EventArgs eventArgs)
	{
		Interaction.ExecuteActions(this, Actions, eventArgs);
	}
}
