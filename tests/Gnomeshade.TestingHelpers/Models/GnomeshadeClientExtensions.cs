// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

using NodaTime;

namespace Gnomeshade.TestingHelpers.Models;

public static class GnomeshadeClientExtensions
{
	public static async Task<Counterparty> CreateCounterpartyAsync(this IGnomeshadeClient accountClient)
	{
		var counterpartyId = Guid.NewGuid();
		var counterparty = new CounterpartyCreation { Name = $"{counterpartyId:N}" };
		await accountClient.PutCounterpartyAsync(counterpartyId, counterparty);
		return await accountClient.GetCounterpartyAsync(counterpartyId);
	}

	public static async Task<Category> CreateCategoryAsync(this IGnomeshadeClient productClient)
	{
		var categoryId = Guid.NewGuid();
		var category = new CategoryCreation { Name = $"{categoryId:N}" };
		await productClient.PutCategoryAsync(categoryId, category);
		return await productClient.GetCategoryAsync(categoryId);
	}

	public static async Task<Transaction> CreateTransactionAsync(this ITransactionClient transactionClient)
	{
		var transactionId = Guid.NewGuid();
		var transaction = new TransactionCreation { BookedAt = SystemClock.Instance.GetCurrentInstant() };
		await transactionClient.PutTransactionAsync(transactionId, transaction);
		return await transactionClient.GetTransactionAsync(transactionId);
	}
}
