using System;
using System.Collections.Generic;

namespace Tracking.Finance.Web.Models.Transactions
{
	public class TransactionIndexViewModel
	{
		public TransactionIndexViewModel(
			int id,
			int? sourceAccountId,
			string sourceAccountName,
			int? targetAccountId,
			string targetAccountName,
			DateTime completedAt,
			decimal sourceAmount,
			string sourceCurrency,
			decimal targetAmount,
			string targetCurrency)
		{
			Id = id;
			SourceAccountId = sourceAccountId;
			SourceAccountName = sourceAccountName;
			TargetAccountId = targetAccountId;
			TargetAccountName = targetAccountName;
			CompletedAt = completedAt;
			SourceAmount = sourceAmount;
			SourceCurrency = sourceCurrency;
			TargetAmount = targetAmount;
			TargetCurrency = targetCurrency;
		}

		public TransactionIndexViewModel(
			int id,
			int? sourceAccountId,
			string sourceAccountName,
			int? targetAccountId,
			string targetAccountName,
			DateTime date,
			List<CurrencyAmount> currencies)
		{
			Id = id;
			SourceAccountId = sourceAccountId;
			SourceAccountName = sourceAccountName;
			TargetAccountId = targetAccountId;
			TargetAccountName = targetAccountName;
			CompletedAt = date;
			Currencies = currencies;
		}

		public int Id { get; }

		public int? SourceAccountId { get; }

		public string SourceAccountName { get; }

		public int? TargetAccountId { get; }

		public string TargetAccountName { get; }

		public DateTime CompletedAt { get; }

		public decimal SourceAmount { get; }

		public string SourceCurrency { get; }

		public decimal TargetAmount { get; }

		public string TargetCurrency { get; }

		public List<CurrencyAmount> Currencies { get; }
	}
}
