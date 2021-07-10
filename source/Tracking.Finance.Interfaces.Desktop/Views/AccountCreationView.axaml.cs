// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Tracking.Finance.Interfaces.Desktop.Views
{
	public sealed class AccountCreationView : UserControl
	{
		private readonly TextBox _accountName;

		/// <summary>
		/// Initializes a new instance of the <see cref="AccountCreationView"/> class.
		/// </summary>
		public AccountCreationView()
		{
			AvaloniaXamlLoader.Load(this);

			_accountName = this.FindControl<TextBox>("AccountName");
			_accountName.AttachedToVisualTree += OnAccountNameAttachedToVisualTree;
		}

		private void OnAccountNameAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs eventArgs)
		{
			_accountName.Focus();
		}
	}
}
