﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Logging;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Repositories;

/// <summary>Persistence store of <see cref="AccessEntity"/>.</summary>
public sealed class AccessRepository
{
	private readonly ILogger<AccessRepository> _logger;
	private readonly DbConnection _dbConnection;

	/// <summary>Initializes a new instance of the <see cref="AccessRepository"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public AccessRepository(ILogger<AccessRepository> logger, DbConnection dbConnection)
	{
		_logger = logger;
		_dbConnection = dbConnection;
	}

	/// <summary>Gets all accesses.</summary>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all accesses.</returns>
	public Task<IEnumerable<AccessEntity>> GetAllAsync(CancellationToken cancellationToken)
	{
		_logger.GetAll();
		const string sql = "SELECT id AS Id, created_at CreatedAt, name AS Name, normalized_name NormalizedName FROM access;";
		var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
		return _dbConnection.QueryAsync<AccessEntity>(command);
	}
}
