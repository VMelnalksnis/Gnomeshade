// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.WebApi.Client;

namespace Gnomeshade.Avalonia.Core.Transactions;

/// <summary>Base class for transaction upsertion view models.</summary>
public abstract class TransactionUpsertionBase : UpsertionViewModel
{
	/// <summary>Initializes a new instance of the <see cref="TransactionUpsertionBase"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="id">The id of the transaction to edit.</param>
	protected TransactionUpsertionBase(IActivityService activityService, IGnomeshadeClient gnomeshadeClient, Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		Id = id;
	}
}
