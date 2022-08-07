// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;

using AutoMapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Importing;
using Gnomeshade.WebApi.Models.Transactions;

namespace Gnomeshade.WebApi.V1_0.Importing.Results;

internal sealed class AccountReportResultBuilder
{
	private readonly Mapper _mapper;

	internal AccountReportResultBuilder(Mapper mapper, AccountEntity userAccount, bool created)
	{
		_mapper = mapper;

		var model = _mapper.Map<Account>(userAccount);
		ReportResult = new() { UserAccount = model };
		AddAccount(userAccount, created);
	}

	private AccountReportResult ReportResult { get; set; }

	internal void AddAccount(AccountEntity account, bool created)
	{
		if (ReportResult.AccountReferences.Any(reference => reference.Account.Name == account.Name))
		{
			return;
		}

		var model = _mapper.Map<Account>(account);
		ReportResult.AccountReferences.Add(new() { Account = model, Created = created });
	}

	internal void AddTransfer(TransferEntity transfer, bool created)
	{
		if (ReportResult.TransferReferences.Any(reference => reference.Transfer.Id == transfer.Id))
		{
			return;
		}

		var model = _mapper.Map<Transfer>(transfer);
		ReportResult.TransferReferences.Add(new() { Transfer = model, Created = created });
	}

	internal void AddTransaction(TransactionEntity transaction, bool created)
	{
		if (ReportResult.TransactionReferences.Any(reference => reference.Transaction.Id == transaction.Id))
		{
			return;
		}

		var model = _mapper.Map<Transaction>(transaction);
		ReportResult.TransactionReferences.Add(new() { Transaction = model, Created = created });
	}

	internal AccountReportResult ToResult()
	{
		var result = ReportResult;
		ReportResult = new();
		return result;
	}
}
