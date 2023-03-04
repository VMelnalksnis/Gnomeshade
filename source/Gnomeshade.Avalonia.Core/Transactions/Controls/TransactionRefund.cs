// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Transactions;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions.Controls;

/// <summary>Marks a transaction as a refund of another transaction.</summary>
public sealed partial class TransactionRefund : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	/// <summary>Gets or sets the transaction that refunds <see cref="Target"/>.</summary>
	[Notify]
	private TransactionOverview? _source;

	/// <summary>Gets or sets the transaction that is refunded by <see cref="Source"/>.</summary>
	[Notify]
	private TransactionOverview? _target;

	/// <summary>Initializes a new instance of the <see cref="TransactionRefund"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	public TransactionRefund(IActivityService activityService, IGnomeshadeClient gnomeshadeClient)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
	}

	/// <summary>Gets a value indicating whether the state is valid to call <see cref="RefundAsync"/>.</summary>
	public bool CanRefund => Source is not null && Target is not null && Source.Id != Target.Id;

	/// <summary>Marks <see cref="Source"/> as a transaction that refunds <see cref="Target"/>.</summary>
	/// <exception cref="InvalidOperationException"><see cref="CanRefund"/> is <c>false</c>.</exception>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task RefundAsync()
	{
		if (Target is null || Source is null || !CanRefund)
		{
			throw new InvalidOperationException();
		}

		var target = await _gnomeshadeClient.GetTransactionAsync(Target.Id);
		var creation = new TransactionCreation
		{
			OwnerId = target.OwnerId,
			BookedAt = target.BookedAt,
			ValuedAt = target.ValuedAt,
			Description = target.Description,
			ReconciledAt = target.ReconciledAt,
			ImportedAt = target.ImportedAt,
			RefundedBy = Source.Id,
		};

		await _gnomeshadeClient.PutTransactionAsync(target.Id, creation);
	}
}
