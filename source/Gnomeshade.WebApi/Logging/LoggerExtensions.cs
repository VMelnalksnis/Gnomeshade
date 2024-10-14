// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Text.Json;

using Microsoft.Extensions.Logging;

using VMelnalksnis.NordigenDotNet.Accounts;

using static Microsoft.Extensions.Logging.LogLevel;

namespace Gnomeshade.WebApi.Logging;

/// <summary>Helper methods for logging.</summary>
internal static partial class LoggerExtensions
{
	[LoggerMessage(1, Debug, "Getting requisition for {InstitutionId}")]
	internal static partial void GettingRequisition(this ILogger logger, string institutionId);

	[LoggerMessage(2, Debug, "Creating new requisition for {InstitutionId}")]
	internal static partial void CreatingRequisition(this ILogger logger, string institutionId);

	[LoggerMessage(3, Debug, "Matched report account to {AccountName}")]
	internal static partial void MatchedReportAccount(this ILogger logger, string accountName);

	[LoggerMessage(4, Trace, "Parsing transaction {ServicerReference}; {BookedTransaction}")]
	internal static partial void ParsingTransaction(this ILogger logger, string servicerReference, string bookedTransaction);

	internal static void ParsingTransaction(this ILogger logger, BookedTransaction transaction)
	{
		var json = JsonSerializer.Serialize(transaction, LoggingSerializerContext.Default.BookedTransaction);
		logger.ParsingTransaction(transaction.TransactionId, json);
	}
}
