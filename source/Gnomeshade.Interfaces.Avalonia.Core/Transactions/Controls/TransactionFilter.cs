// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Controls;

/// <summary>Values for filtering transactions.</summary>
public sealed class TransactionFilter : ViewModelBase
{
	private static readonly string[] _isValidNames = { nameof(IsValid) };

	private DateTimeOffset? _fromDate;
	private DateTimeOffset? _toDate;

	/// <summary>Gets or sets the date from which to get transactions.</summary>
	public DateTimeOffset? FromDate
	{
		get => _fromDate;
		set => SetAndNotifyWithGuard(ref _fromDate, value, nameof(FromDate), _isValidNames);
	}

	/// <summary>Gets or sets the date until which to ge transactions.</summary>
	public DateTimeOffset? ToDate
	{
		get => _toDate;
		set => SetAndNotifyWithGuard(ref _toDate, value, nameof(ToDate), _isValidNames);
	}

	/// <summary>Gets a value indicating whether the current values are valid search parameters.</summary>
	public bool IsValid => ToDate is null || FromDate is null || ToDate >= FromDate;
}
