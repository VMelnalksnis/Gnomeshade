// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Interfaces.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.Interfaces.Avalonia.Core.Transactions.Controls;

/// <summary>Transaction information besides transaction items.</summary>
public sealed class TransactionProperties : ViewModelBase
{
	private static readonly string[] _isValid = { nameof(IsValid) };

	private DateTimeOffset? _bookingDate;
	private TimeSpan? _bookingTime;
	private DateTimeOffset? _valueDate;
	private TimeSpan? _valueTime;
	private DateTimeOffset? _reconciliationDate;
	private TimeSpan? _reconciliationTime;
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
	public ZonedDateTime? BookedAt => BookingDate.HasValue
		? new LocalDateTime(
				BookingDate.Value.Year,
				BookingDate.Value.Month,
				BookingDate.Value.Day,
				BookingTime.GetValueOrDefault().Hours,
				BookingTime.GetValueOrDefault().Minutes)
			.InZoneStrictly(DateTimeZoneProviders.Tzdb.GetSystemDefault())
		: null;

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
	public ZonedDateTime? ValuedAt => ValueDate.HasValue
		? new LocalDateTime(
				ValueDate.Value.Year,
				ValueDate.Value.Month,
				ValueDate.Value.Day,
				ValueTime.GetValueOrDefault().Hours,
				ValueTime.GetValueOrDefault().Minutes)
			.InZoneStrictly(DateTimeZoneProviders.Tzdb.GetSystemDefault())
		: null;

	/// <summary>Gets or sets the date on which the transaction was reconciled.</summary>
	public DateTimeOffset? ReconciliationDate
	{
		get => _reconciliationDate;
		set => SetAndNotifyWithGuard(ref _reconciliationDate, value, nameof(ReconciliationDate), _isValid);
	}

	/// <summary>Gets or sets the time when the transaction was reconciled.</summary>
	public TimeSpan? ReconciliationTime
	{
		get => _reconciliationTime;
		set => SetAndNotifyWithGuard(ref _reconciliationTime, value, nameof(ReconciliationTime), _isValid);
	}

	/// <inheritdoc cref="Transaction.ReconciledAt"/>
	public ZonedDateTime? ReconciledAt => ReconciliationDate.HasValue
		? new LocalDateTime(
				ReconciliationDate.Value.Year,
				ReconciliationDate.Value.Month,
				ReconciliationDate.Value.Day,
				ReconciliationTime.GetValueOrDefault().Hours,
				ReconciliationTime.GetValueOrDefault().Minutes)
			.InZoneStrictly(DateTimeZoneProviders.Tzdb.GetSystemDefault())
		: null;

	/// <summary>Gets or sets the description of the transaction.</summary>
	public string? Description
	{
		get => _description;
		set => SetAndNotify(ref _description, value, nameof(Description));
	}

	/// <summary>Gets a value indicating whether the current value of other properties are valid for a transaction.</summary>
	public bool IsValid =>
		((BookingDate.HasValue && BookingTime.HasValue) ||
		(ValueDate.HasValue && ValueTime.HasValue)) &&
		((ReconciledAt is null && ReconciliationTime is null) || ReconciledAt is not null);
}
