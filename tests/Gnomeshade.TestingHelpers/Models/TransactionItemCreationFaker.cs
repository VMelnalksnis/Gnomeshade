// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Bogus;

using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.TestingHelpers.Models;

public sealed class TransactionItemCreationFaker : Faker<TransactionItemCreationModel>
{
	public TransactionItemCreationFaker(Guid sourceAccountId, Guid targetAccountId, Guid productId)
	{
		RuleFor(item => item.SourceAmount, faker => faker.Finance.Amount());
		RuleFor(item => item.SourceAccountId, sourceAccountId);
		RuleFor(item => item.TargetAmount, faker => faker.Finance.Amount());
		RuleFor(item => item.TargetAccountId, targetAccountId);
		RuleFor(item => item.ProductId, productId);
		RuleFor(item => item.Amount, faker => faker.Random.Decimal(0, 10));
		RuleFor(item => item.BankReference, faker => faker.Finance.Account(16).OrNull(faker));
		RuleFor(item => item.ExternalReference, faker => faker.Finance.Account(16).OrNull(faker));
		RuleFor(item => item.InternalReference, faker => faker.Finance.Account(16).OrNull(faker));
		RuleFor(item => item.DeliveryDate, faker => faker.Date.Recent().OrNull(faker));
		RuleFor(item => item.Description, faker => faker.Lorem.Sentence().OrNull(faker));
	}
}
