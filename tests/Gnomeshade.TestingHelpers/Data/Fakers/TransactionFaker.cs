// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.TestingHelpers.Data.Fakers;

public sealed class TransactionFaker : ModifiableEntityFaker<TransactionEntity>
{
	public TransactionFaker(UserEntity user)
		: this(user.Id)
	{
	}

	public TransactionFaker(Guid userId)
		: base(userId)
	{
		RuleFor(transaction => transaction.BookedAt, faker => faker.Date.RecentUtc());
		RuleFor(transaction => transaction.ValuedAt, faker => faker.Date.RecentUtc());
		RuleFor(transaction => transaction.Description, faker => faker.Lorem.Sentence());
		RuleFor(transaction => transaction.ImportedAt, faker => faker.Date.RecentUtc());
		RuleFor(transaction => transaction.ReconciledAt, faker => faker.Date.RecentUtc());
		RuleFor(transaction => transaction.ReconciledByUserId, userId);
	}
}
