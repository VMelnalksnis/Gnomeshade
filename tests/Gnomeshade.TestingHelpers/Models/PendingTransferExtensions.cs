// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.TestingHelpers.Models;

public static class PendingTransferExtensions
{
	public static PendingTransferCreation ToCreation(this PendingTransfer transfer) => new()
	{
		SourceAccountId = transfer.SourceAccountId,
		TargetCounterpartyId = transfer.TargetCounterpartyId,
		SourceAmount = transfer.SourceAmount,
		TransferId = transfer.TransferId,
	};
}
