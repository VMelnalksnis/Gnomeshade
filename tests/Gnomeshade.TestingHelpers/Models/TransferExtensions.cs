// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.TestingHelpers.Models;

public static class TransferExtensions
{
	public static TransferCreation ToCreation(this Transfer transfer) => new()
	{
		TransactionId = transfer.TransactionId,
		SourceAccountId = transfer.SourceAccountId,
		TargetAccountId = transfer.TargetAccountId,
		SourceAmount = transfer.SourceAmount,
		TargetAmount = transfer.TargetAmount,
		BankReference = transfer.BankReference,
		ExternalReference = transfer.ExternalReference,
		InternalReference = transfer.InternalReference,
	};
}
