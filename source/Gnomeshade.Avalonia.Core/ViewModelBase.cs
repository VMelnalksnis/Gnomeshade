// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Base class for all view models.</summary>
public abstract class ViewModelBase : PropertyChangedBase
{
	private bool _isBusy;

	/// <summary>Gets or sets a value indicating whether the viewmodel busy.</summary>
	public bool IsBusy
	{
		get => _isBusy;
		protected set => SetAndNotify(ref _isBusy, value);
	}

	/// <summary>Refreshes all data loaded from API.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task RefreshAsync()
	{
		IsBusy = true;
		await Refresh().ConfigureAwait(false);
		IsBusy = false;
	}

	/// <summary>Refreshes all data loaded from API.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	protected virtual Task Refresh() => Task.CompletedTask;
}
