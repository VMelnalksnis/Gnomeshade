﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;

using AutoMapper;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Importing;
using Gnomeshade.Interfaces.WebApi.Models.Products;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Importing.Results
{
	public sealed class AccountReportResultBuilder
	{
		private readonly Mapper _mapper;

		public AccountReportResultBuilder(Mapper mapper, Data.Models.Account userAccount, bool created)
		{
			_mapper = mapper;

			var model = _mapper.Map<Account>(userAccount);
			ReportResult = new() { UserAccount = model };
			AddAccount(userAccount, created);
		}

		private AccountReportResult ReportResult { get; set; }

		public void AddAccount(Data.Models.Account account, bool created)
		{
			if (ReportResult.AccountReferences.Any(reference => reference.Account.Name == account.Name))
			{
				return;
			}

			var model = _mapper.Map<Account>(account);
			ReportResult.AccountReferences.Add(new() { Account = model, Created = created });
		}

		public void AddProduct(Data.Models.Product product, bool created)
		{
			if (ReportResult.ProductReferences.Any(reference => reference.Product.Name == product.Name))
			{
				return;
			}

			var model = _mapper.Map<ProductModel>(product);
			ReportResult.ProductReferences.Add(new() { Product = model, Created = created });
		}

		public void AddTransaction(Data.Models.Transaction transaction, bool created)
		{
			if (ReportResult.TransactionReferences.Any(reference => reference.Transaction.Id == transaction.Id))
			{
				return;
			}

			var model = _mapper.Map<TransactionModel>(transaction);
			ReportResult.TransactionReferences.Add(new() { Transaction = model, Created = created });
		}

		public AccountReportResult ToResult()
		{
			var result = ReportResult;
			ReportResult = new();
			return result;
		}
	}
}
