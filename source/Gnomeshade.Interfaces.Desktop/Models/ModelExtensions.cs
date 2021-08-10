﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Gnomeshade.Interfaces.WebApi.V1_0.Accounts;
using Gnomeshade.Interfaces.WebApi.V1_0.Products;
using Gnomeshade.Interfaces.WebApi.V1_0.Transactions;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.Desktop.Models
{
	/// <summary>
	/// Extension methods for translating API models to grid row models.
	/// </summary>
	public static class ModelExtensions
	{
		[LinqTunnel]
		[Pure]
		public static IEnumerable<AccountOverviewRow> Translate(this IEnumerable<AccountModel> accounts)
		{
			return accounts
				.SelectMany(account => account.Currencies.Select(inCurrency => (account, inCurrency)))
				.Select(tuple => new AccountOverviewRow
				{
					Name = tuple.account.Name,
					Currency = tuple.inCurrency.Currency.AlphabeticCode,
					Disabled = tuple.inCurrency.Disabled,
				});
		}

		[LinqTunnel]
		[Pure]
		public static IEnumerable<TransactionOverview> Translate(
			this IEnumerable<TransactionModel> transactions,
			IReadOnlyCollection<AccountModel> accounts)
		{
			return transactions
				.Select(transaction =>
				{
					var firstItem = transaction.Items.First();
					var sourceAccount = accounts.Single(account =>
						account.Currencies.Any(currency => currency.Id == firstItem.SourceAccountId));
					var targetAccount = accounts.Single(account =>
						account.Currencies.Any(currency => currency.Id == firstItem.TargetAccountId));

					return new TransactionOverview
					{
						TransactionModel = transaction,
						Id = transaction.Id,
						Date = transaction.Date.LocalDateTime,
						Description = transaction.Description,
						SourceAccount = sourceAccount.Name,
						TargetAccount = targetAccount.Name,
						SourceAmount = transaction.Items.Sum(item => item.SourceAmount), // todo select per currency
						TargetAmount = transaction.Items.Sum(item => item.TargetAmount),
					};
				});
		}

		[LinqTunnel]
		[Pure]
		public static IEnumerable<TransactionItemOverviewRow> Translate(this IEnumerable<TransactionItemModel> items)
		{
			return items
				.Select(item => new TransactionItemOverviewRow
				{
					SourceAmount = item.SourceAmount,
					TargetAmount = item.TargetAmount,
					Product = item.Product.Name,
					Amount = item.Amount,
					Description = item.Description,
				});
		}


		[LinqTunnel]
		[Pure]
		public static IEnumerable<ProductOverviewRow> Translate(this IEnumerable<ProductModel> products)
		{
			return products
				.Select(product => new ProductOverviewRow
				{
					Name = product.Name,
				});
		}
	}
}
