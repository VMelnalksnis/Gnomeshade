// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

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
	private LocalDateTime? _reconciliationDate;

	/// <summary>Gets or sets a value indicating whether the transaction is reconciled.</summary>
	[Notify]
	private bool _reconciled;

	/// <summary>Gets or sets the date on which the transaction was imported.</summary>
	[Notify]
	private LocalDateTime? _importDate;

	/// <summary>Gets or sets the date on which the transaction was refunded.</summary>
	[Notify]
	private LocalDateTime? _refundDate;

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
	public ZonedDateTime? ReconciledAt =>
		ReconciliationDate?.InZoneStrictly(DateTimeZoneProviders.Tzdb.GetSystemDefault());

	/// <inheritdoc cref="Transaction.ReconciledAt"/>
	public ZonedDateTime? ImportedAt => ImportDate?.InZoneStrictly(DateTimeZoneProviders.Tzdb.GetSystemDefault());

	/// <summary>Gets a value indicating whether the transaction was imported.</summary>
	public bool IsImported => ImportedAt is not null;

	/// <summary>Gets a value indicating whether the current value of other properties are valid for a transaction.</summary>
	public bool IsValid => !(Reconciled && ReconciledAt is null);
}
