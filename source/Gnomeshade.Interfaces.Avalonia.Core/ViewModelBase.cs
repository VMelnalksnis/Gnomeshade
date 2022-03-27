// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

namespace Gnomeshade.Interfaces.Avalonia.Core;

/// <summary>Base class for all view models.</summary>
public abstract class ViewModelBase : PropertyChangedBase
{
	/// <summary>Refreshes all data loaded from API.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public virtual Task RefreshAsync() => Task.CompletedTask;
}
