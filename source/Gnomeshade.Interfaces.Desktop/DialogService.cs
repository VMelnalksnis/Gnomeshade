// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core;
using Gnomeshade.Interfaces.Desktop.Views;

namespace Gnomeshade.Interfaces.Desktop;

/// <inheritdoc />
public sealed class DialogService : IDialogService
{
	private readonly ViewLocator<App> _viewLocator;

	/// <summary>Initializes a new instance of the <see cref="DialogService"/> class.</summary>
	/// <param name="viewLocator">View locator for dialog view initialization.</param>
	public DialogService(ViewLocator<App> viewLocator)
	{
		_viewLocator = viewLocator;
	}

	/// <inheritdoc />
	public Task ShowDialog<TViewModel>(Window owner, TViewModel viewModel, Action<Window> dialogAction)
		where TViewModel : ViewModelBase
	{
		var dialogWindow = CreateDialog(viewModel, dialogAction);
		return dialogWindow.ShowDialog(owner);
	}

	/// <inheritdoc />
	public Task<TResult?> ShowDialog<TViewModel, TResult>(
		Window owner,
		TViewModel viewModel,
		Action<Window> dialogAction)
		where TViewModel : ViewModelBase
		where TResult : class
	{
		var dialogWindow = CreateDialog(viewModel, dialogAction);
		return dialogWindow.ShowDialog<TResult?>(owner);
	}

	/// <inheritdoc />
	public Task<TResult?> ShowDialogValue<TViewModel, TResult>(
		Window owner,
		TViewModel viewModel,
		Action<Window> dialogAction)
		where TViewModel : ViewModelBase
		where TResult : struct
	{
		var dialogWindow = CreateDialog(viewModel, dialogAction);
		return dialogWindow.ShowDialog<TResult?>(owner);
	}

	private Window CreateDialog<TViewModel>(TViewModel viewModel, Action<Window> dialogAction)
		where TViewModel : ViewModelBase
	{
		var viewControl = _viewLocator.Build(viewModel);
		viewControl.DataContext = viewModel;

		var dialog = new DialogWindow { Width = 400, Height = 300 };
		if (dialog.Content is Panel panel)
		{
			panel.Children.Add(viewControl);
		}
		else
		{
			dialog.Content = viewControl;
		}

		dialogAction(dialog);
		return dialog;
	}
}
