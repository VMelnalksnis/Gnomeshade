// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Gnomeshade.Avalonia.Core.Counterparties;

internal sealed class CounterpartyComparer : IComparer, IComparer<CounterpartyRow>
{
	private static readonly StringComparer _comparer = StringComparer.Ordinal;

	/// <inheritdoc />
	public int Compare(object? x, object? y)
	{
		return Compare(x as CounterpartyRow, y as CounterpartyRow);
	}

	/// <inheritdoc />
	public int Compare(CounterpartyRow? x, CounterpartyRow? y)
	{
		return _comparer.Compare(x?.Name, y?.Name);
	}
}
