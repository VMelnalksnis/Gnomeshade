// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Transactions
{
	public record TransactionItemModel(
		Guid Id,
		Guid UserId,
		Guid TransactionId,
		decimal SourceAmount,
		Guid SourceAccountId,
		decimal TargetAmount,
		Guid TargetAccountId,
		DateTimeOffset CreatedAt,
		Guid CreatedByUserId,
		DateTimeOffset ModifiedAt,
		Guid ModifiedByUserId,
		Guid ProductId,
		decimal Amount,
		string? BankReference,
		string? ExternalReference,
		string? InternalReference,
		DateTimeOffset? DeliveryDate,
		string? Description);
}
