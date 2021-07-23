// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Tracking.Finance.Data.Models;

namespace Tracking.Finance.Data.TestingHelpers
{
	public sealed class TransactionFaker : ModifiableEntityFaker<Transaction>
	{
		public TransactionFaker(User user)
			: this(user.Id)
		{
		}

		public TransactionFaker(Guid userId)
			: base(userId)
		{
			RuleFor(transaction => transaction.Date, faker => faker.Date.Recent());
			RuleFor(transaction => transaction.Description, faker => faker.Lorem.Sentence());
			RuleFor(transaction => transaction.Generated, true);
			RuleFor(transaction => transaction.Validated, false);
			RuleFor(transaction => transaction.Completed, false);
		}
	}
}
