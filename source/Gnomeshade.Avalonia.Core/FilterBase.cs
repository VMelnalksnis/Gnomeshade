// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Gnomeshade.Avalonia.Core;

/// <summary>Base class for view models used for filtering <see cref="OverviewViewModel{TRow,TUpsertion}"/>.</summary>
/// <typeparam name="TRow">The row type to filter.</typeparam>
public abstract class FilterBase<TRow> : ViewModelBase
{
	/// <summary>Predicate for determining if an item is suitable for inclusion in the view.</summary>
	/// <param name="item">The item to check against the filters set in this viewmodel.</param>
	/// <returns><see langword="true"/> if <paramref name="item"/> matches the filters set in this viewmodel; otherwise <see langword="false"/>.</returns>
	public bool Filter(object item) => item is TRow row && FilterRow(row);

	/// <summary>Strongly typed predicate for determining if an item is suitable for inclusion in the view.</summary>
	/// <param name="row">The row to check against the filters set in this viewmodel.</param>
	/// <returns><see langword="true"/> if <paramref name="row"/> matches the filters set in this viewmodel; otherwise <see langword="false"/>.</returns>
	protected abstract bool FilterRow(TRow row);
}
