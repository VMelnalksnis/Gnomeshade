// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Entities;

namespace Gnomeshade.TestingHelpers.Data.Fakers;

public sealed class TransactionItemFaker : ModifiableEntityFaker<TransactionItemEntity>
{
	public TransactionItemFaker(
		UserEntity user,
		TransactionEntity transaction,
		AccountInCurrencyEntity source,
		AccountInCurrencyEntity target,
		ProductEntity product)
		: this(user.Id, transaction.Id, source.Id, target.Id, product.Id)
	{
		RuleFor(item => item.Product, product);
	}

	public TransactionItemFaker(
		Guid userId,
		Guid transactionId,
		Guid sourceAccountId,
		Guid targetAccountId,
		Guid productId)
		: base(userId)
	{
		RuleFor(item => item.TransactionId, transactionId);
		RuleFor(item => item.SourceAmount, faker => faker.Random.Decimal(0, 500));
		RuleFor(item => item.SourceAccountId, sourceAccountId);
		RuleFor(item => item.TargetAmount, (_, item) => item.SourceAmount);
		RuleFor(item => item.TargetAccountId, targetAccountId);
		RuleFor(item => item.ProductId, productId);
		RuleFor(item => item.Amount, faker => faker.Random.Decimal(0, 10));
		RuleFor(item => item.ExternalReference, faker => faker.Finance.Account());
		RuleFor(item => item.InternalReference, faker => faker.Finance.Account());
		RuleFor(item => item.DeliveryDate, faker => faker.Date.RecentUtc());
		RuleFor(item => item.Description, faker => faker.Lorem.Sentence());
	}
}
