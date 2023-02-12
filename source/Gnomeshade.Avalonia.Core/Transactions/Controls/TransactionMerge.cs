// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions.Controls;

/// <summary>Merges one transaction into another.</summary>
public sealed partial class TransactionMerge : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	/// <summary>Gets or sets the transaction which to merge into <see cref="Target"/>.</summary>
	[Notify]
	private TransactionOverview? _source;

	/// <summary>Gets or sets the transaction into which to merge the <see cref="Source"/>.</summary>
	[Notify]
	private TransactionOverview? _target;

	/// <summary>Initializes a new instance of the <see cref="TransactionMerge"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	public TransactionMerge(IActivityService activityService, IGnomeshadeClient gnomeshadeClient)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
	}

	/// <summary>Gets a value indicating whether the state is valid to call <see cref="MergeAsync"/>.</summary>
	public bool CanMerge => Source is not null && Target is not null && Source.Id != Target.Id;

	/// <summary>Merges <see cref="Source"/> transaction into <see cref="Target"/>.</summary>
	/// <exception cref="InvalidOperationException"><see cref="CanMerge"/> is <c>false</c>.</exception>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	/// <seealso cref="ITransactionClient.MergeTransactionsAsync"/>
	public async Task MergeAsync()
	{
		if (!CanMerge)
		{
			throw new InvalidOperationException();
		}

		await _gnomeshadeClient.MergeTransactionsAsync(Target!.Id, Source!.Id);
	}
}
