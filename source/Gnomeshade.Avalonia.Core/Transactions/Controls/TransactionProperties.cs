// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using Avalonia.Controls;

using Gnomeshade.WebApi.Models.Owners;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions.Controls;

/// <summary>Transaction information besides transaction items.</summary>
public sealed partial class TransactionProperties : ViewModelBase
{
	/// <summary>Gets or sets the date on which the transaction was reconciled.</summary>
	[Notify]
	private DateTimeOffset? _reconciliationDate;

	/// <summary>Gets or sets the time when the transaction was reconciled.</summary>
	[Notify]
	private TimeSpan? _reconciliationTime;

	/// <summary>Gets or sets a value indicating whether the transaction is reconciled.</summary>
	[Notify]
	private bool _reconciled;

	/// <summary>Gets or sets the date on which the transaction was imported.</summary>
	[Notify]
	private DateTimeOffset? _importDate;

	/// <summary>Gets or sets the time when the transaction was imported.</summary>
	[Notify]
	private TimeSpan? _importTime;

	/// <summary>Gets or sets the date on which the transaction was refunded.</summary>
	[Notify]
	private DateTimeOffset? _refundDate;

	/// <summary>Gets or sets the time when the transaction was refunded.</summary>
	[Notify]
	private TimeSpan? _refundTime;

	/// <summary>Gets or sets the description of the transaction.</summary>
	[Notify]
	private string? _description;

	/// <summary>Gets a collection of available owners.</summary>
	[Notify(Setter.Internal)]
	private List<Owner> _owners;

	/// <summary>Gets or sets the owner of the account.</summary>
	[Notify]
	private Owner? _owner;

	/// <summary>Initializes a new instance of the <see cref="TransactionProperties"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	public TransactionProperties(IActivityService activityService)
		: base(activityService)
	{
		_owners = new();
	}

	/// <summary>Gets a delegate for formatting a owner in an <see cref="AutoCompleteBox"/>.</summary>
	public AutoCompleteSelector<object> OwnerSelector => AutoCompleteSelectors.Owner;

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

	/// <inheritdoc cref="Transaction.ReconciledAt"/>
	public ZonedDateTime? ImportedAt => ImportDate.HasValue
		? new LocalDateTime(
				ImportDate.Value.Year,
				ImportDate.Value.Month,
				ImportDate.Value.Day,
				ImportTime.GetValueOrDefault().Hours,
				ImportTime.GetValueOrDefault().Minutes)
			.InZoneStrictly(DateTimeZoneProviders.Tzdb.GetSystemDefault())
		: null;

	/// <summary>Gets a value indicating whether the transaction was imported.</summary>
	public bool IsImported => ImportedAt is not null;

	/// <summary>Gets a value indicating whether the current value of other properties are valid for a transaction.</summary>
	public bool IsValid => !Reconciled && ((ReconciledAt is null && ReconciliationTime is null) || ReconciledAt is not null);
}
