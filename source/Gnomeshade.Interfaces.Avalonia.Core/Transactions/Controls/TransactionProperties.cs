// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Controls;

/// <summary>Transaction information besides transaction items.</summary>
public sealed class TransactionProperties : ViewModelBase
{
	private static readonly string[] _isValid = { nameof(IsValid) };

	private DateTimeOffset? _bookingDate;
	private TimeSpan? _bookingTime;
	private DateTimeOffset? _valueDate;
	private TimeSpan? _valueTime;
	private string? _description;

	/// <summary>Gets or sets the date on which the transaction was posted to an account on the account servicer accounting books.</summary>
	public DateTimeOffset? BookingDate
	{
		get => _bookingDate;
		set => SetAndNotifyWithGuard(ref _bookingDate, value, nameof(BookingDate), _isValid);
	}

	/// <summary>Gets or sets the time at which the transaction was posted to an account on the account servicer accounting books.</summary>
	public TimeSpan? BookingTime
	{
		get => _bookingTime;
		set => SetAndNotifyWithGuard(ref _bookingTime, value, nameof(BookingTime), _isValid);
	}

	/// <inheritdoc cref="Transaction.BookedAt"/>
	public DateTimeOffset? BookedAt => BookingDate.HasValue
		? new DateTimeOffset(BookingDate.Value.Date).Add(BookingTime.GetValueOrDefault())
		: default(DateTimeOffset?);

	/// <summary>Gets or sets the date on which assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public DateTimeOffset? ValueDate
	{
		get => _valueDate;
		set => SetAndNotifyWithGuard(ref _valueDate, value, nameof(ValueDate), _isValid);
	}

	/// <summary>Gets or sets the time at which assets become available in case of deposit, or when assets cease to be available in case of withdrawal.</summary>
	public TimeSpan? ValueTime
	{
		get => _valueTime;
		set => SetAndNotifyWithGuard(ref _valueTime, value, nameof(ValueTime), _isValid);
	}

	/// <inheritdoc cref="Transaction.ValuedAt"/>
	public DateTimeOffset? ValuedAt => ValueDate.HasValue
		? new DateTimeOffset(ValueDate.Value.Date).Add(ValueTime.GetValueOrDefault())
		: default(DateTimeOffset?);

	/// <summary>Gets or sets the description of the transaction.</summary>
	public string? Description
	{
		get => _description;
		set => SetAndNotify(ref _description, value, nameof(Description));
	}

	/// <summary>Gets a value indicating whether the current value of other properties are valid for a transaction.</summary>
	public bool IsValid =>
		(BookingDate.HasValue && BookingTime.HasValue) ||
		(ValueDate.HasValue && ValueTime.HasValue);
}
