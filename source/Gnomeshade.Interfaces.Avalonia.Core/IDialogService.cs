// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Avalonia.Controls;

namespace Gnomeshade.Interfaces.Avalonia.Core;

/// <summary>Creates modal dialogs.</summary>
public interface IDialogService
{
	/// <summary>Show a modal dialog with the specified view model.</summary>
	/// <param name="owner">The window that will own the dialog.</param>
	/// <param name="viewModel">The view model of the dialogs initial state.</param>
	/// <param name="dialogAction">An action that can be applied to the dialog window before showing it.</param>
	/// <typeparam name="TViewModel">The view model type.</typeparam>
	/// <returns>A task that can be used to track the lifetime of the dialog.</returns>
	public Task ShowDialog<TViewModel>(Window owner, TViewModel viewModel, Action<Window> dialogAction)
		where TViewModel : ViewModelBase;

	/// <summary>Show a modal dialog with the specified view model.</summary>
	/// <param name="owner">The window that will own the dialog.</param>
	/// <param name="viewModel">The view model of the dialogs initial state.</param>
	/// <param name="dialogAction">An action that can be applied to the dialog window before showing it.</param>
	/// <typeparam name="TViewModel">The view model type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task that can be used to track the lifetime of the dialog.</returns>
	public Task<TResult?> ShowDialog<TViewModel, TResult>(Window owner, TViewModel viewModel, Action<Window> dialogAction)
		where TViewModel : ViewModelBase
		where TResult : class;

	/// <summary>Show a modal dialog with the specified view model.</summary>
	/// <param name="owner">The window that will own the dialog.</param>
	/// <param name="viewModel">The view model of the dialogs initial state.</param>
	/// <param name="dialogAction">An action that can be applied to the dialog window before showing it.</param>
	/// <typeparam name="TViewModel">The view model type.</typeparam>
	/// <typeparam name="TResult">The result type.</typeparam>
	/// <returns>A task that can be used to track the lifetime of the dialog.</returns>
	public Task<TResult?> ShowDialogValue<TViewModel, TResult>(Window owner, TViewModel viewModel, Action<Window> dialogAction)
		where TViewModel : ViewModelBase
		where TResult : struct;
}
