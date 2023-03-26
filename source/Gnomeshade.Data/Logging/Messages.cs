// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Gnomeshade.Data.Repositories;

using Microsoft.Extensions.Logging;

using NodaTime;

using static Microsoft.Extensions.Logging.LogLevel;

namespace Gnomeshade.Data.Logging;

/// <summary>Shared logging messages used in this project.</summary>
internal static partial class Messages
{
	[LoggerMessage(EventId = 1, Level = Debug, Message = "Adding entity")]
	internal static partial void AddingEntity(this ILogger logger);

	[LoggerMessage(EventId = 2, Level = Debug, Message = "Adding entity with database transaction")]
	internal static partial void AddingEntityWithTransaction(this ILogger logger);

	[LoggerMessage(EventId = 3, Level = Debug, Message = "Deleting entity {EntityId}")]
	internal static partial void DeletingEntity(this ILogger logger, Guid entityId);

	[LoggerMessage(EventId = 4, Level = Debug, Message = "Deleting entity {EntityId} with database transaction")]
	internal static partial void DeletingEntityWithTransaction(this ILogger logger, Guid entityId);

	[LoggerMessage(EventId = 5, Level = Debug, Message = "Getting all entities")]
	internal static partial void GetAll(this ILogger logger);

	[LoggerMessage(EventId = 6, Level = Debug, Message = "Getting all entities, including deleted: {IncludeDeleted}")]
	internal static partial void GetAll(this ILogger logger, bool includeDeleted);

	[LoggerMessage(EventId = 7, Level = Debug, Message = "Getting entity by {EntityId}")]
	internal static partial void GetId(this ILogger logger, Guid entityId);

	[LoggerMessage(EventId = 8, Level = Debug, Message = "Getting entity by {EntityId} with database transaction")]
	internal static partial void GetIdWithTransaction(this ILogger logger, Guid entityId);

	[LoggerMessage(EventId = 9, Level = Debug, Message = "Finding entity by {EntityId}")]
	internal static partial void FindId(this ILogger logger, Guid entityId);

	[LoggerMessage(EventId = 10, Level = Debug, Message = "Finding entity by {EntityId} with database transaction")]
	internal static partial void FindIdWithTransaction(this ILogger logger, Guid entityId);

	[LoggerMessage(EventId = 11, Level = Debug, Message = "Finding entity by {EntityId} for {AccessLevel}")]
	internal static partial void FindId(this ILogger logger, Guid entityId, AccessLevel accessLevel);

	[LoggerMessage(EventId = 12, Level = Debug, Message = "Finding entity by {EntityId} for {AccessLevel} with database transaction")]
	internal static partial void FindIdWithTransaction(this ILogger logger, Guid entityId, AccessLevel accessLevel);

	[LoggerMessage(EventId = 13, Level = Debug, Message = "Updating entity")]
	internal static partial void UpdatingEntity(this ILogger logger);

	[LoggerMessage(EventId = 14, Level = Debug, Message = "Updating entity with database transaction")]
	internal static partial void UpdatingEntityWithTransaction(this ILogger logger);

	[LoggerMessage(EventId = 15, Level = Debug, Message = "Finding entity by {Iban} with database transaction")]
	internal static partial void FindByIbanWithTransaction(this ILogger logger, string iban);

	[LoggerMessage(EventId = 16, Level = Debug, Message = "Finding entity by {Bic} with database transaction")]
	internal static partial void FindByBicWithTransaction(this ILogger logger, string bic);

	[LoggerMessage(EventId = 17, Level = Debug, Message = "Getting balance for {EntityId}")]
	internal static partial void GetBalance(this ILogger logger, Guid entityId);

	[LoggerMessage(EventId = 18, Level = Debug, Message = "Merging {SourceId} counterparty into {TargetId}")]
	internal static partial void MergeCounterparties(this ILogger logger, Guid sourceId, Guid targetId);

	[LoggerMessage(EventId = 19, Level = Debug, Message = "Finding currency by {AlphabeticCode}")]
	internal static partial void FindAlphabeticCode(this ILogger logger, string alphabeticCode);

	[LoggerMessage(EventId = 20, Level = Debug, Message = "Finding entity by {EntityName}")]
	internal static partial void FindName(this ILogger logger, string entityName);

	[LoggerMessage(EventId = 21, Level = Debug, Message = "Finding entity by {EntityName} with database transaction")]
	internal static partial void FindNameWithTransaction(this ILogger logger, string entityName);

	[LoggerMessage(EventId = 22, Level = Debug, Message = "Merging {SourceId} transaction into {TargetId}")]
	internal static partial void MergeTransactions(this ILogger logger, Guid sourceId, Guid targetId);

	[LoggerMessage(EventId = 23, Level = Debug, Message = "Finding currency by {BankReference}")]
	internal static partial void FindBankReferenceWithTransaction(this ILogger logger, string bankReference);

	[LoggerMessage(EventId = 24, Level = Debug, Message = "Found entity {EntityId}, deleted at {DeletedAt}")]
	internal static partial void FoundEntity(this ILogger logger, Guid entityId, Instant? deletedAt);
}
