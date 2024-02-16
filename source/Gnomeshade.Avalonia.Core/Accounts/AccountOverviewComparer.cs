// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Gnomeshade.Avalonia.Core.Accounts;

internal sealed class AccountOverviewComparer : IComparer, IComparer<AccountOverviewRow>
{
	private static readonly StringComparer _comparer = StringComparer.Ordinal;

	/// <inheritdoc />
	public int Compare(object? x, object? y)
	{
		return Compare(x as AccountOverviewRow, y as AccountOverviewRow);
	}

	/// <inheritdoc />
	public int Compare(AccountOverviewRow? x, AccountOverviewRow? y)
	{
		return _comparer.Compare(x?.Counterparty, y?.Counterparty);
	}
}
