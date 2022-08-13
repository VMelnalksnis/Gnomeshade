// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Bogus;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.Data.Tests.Integration.Fakers;

public sealed class TransactionFaker : ModifiableEntityFaker<TransactionEntity>
{
	public TransactionFaker(UserEntity user)
		: this(user.Id)
	{
	}

	public TransactionFaker(Guid userId)
		: base(userId)
	{
		RuleFor(transaction => transaction.BookedAt, faker => faker.Noda().Instant.Recent());
		RuleFor(transaction => transaction.ValuedAt, faker => faker.Noda().Instant.Recent());
		RuleFor(transaction => transaction.Description, faker => faker.Lorem.Sentence());
		RuleFor(transaction => transaction.ImportedAt, faker => faker.Noda().Instant.Recent());
		RuleFor(transaction => transaction.ReconciledAt, faker => faker.Noda().Instant.Recent());
		RuleFor(transaction => transaction.ReconciledByUserId, userId);
	}
}
