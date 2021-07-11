// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

namespace Tracking.Finance.Interfaces.Desktop.Views
{
	/// <summary>
	/// User control which focuses on an element on creation.
	/// </summary>
	public abstract class FocusOnInitUserControl : UserControl
	{
		protected void Focus(string focusControlName)
		{
			var focusControl = this.FindControl<Control>(focusControlName);
			focusControl.AttachedToVisualTree += (_, _) => focusControl.Focus();
		}
	}
}
