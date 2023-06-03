// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Logging;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Data.Repositories;

/// <summary>Database backed <see cref="CurrencyRepository"/> repository.</summary>
public sealed class CurrencyRepository
{
	private static readonly string _selectAlphabetic = $"{Queries.Currency.SelectAll} WHERE alphabetic_code = @code;";
	private static readonly string _selectId = $"{Queries.Currency.SelectAll} WHERE id = @id;";

	private readonly ILogger<CurrencyRepository> _logger;
	private readonly DbConnection _dbConnection;

	/// <summary>Initializes a new instance of the <see cref="CurrencyRepository"/> class with a database connection.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public CurrencyRepository(ILogger<CurrencyRepository> logger, DbConnection dbConnection)
	{
		_logger = logger;
		_dbConnection = dbConnection;
	}

	/// <summary>Searches for a currency with the specified <see cref="CurrencyEntity.AlphabeticCode"/>.</summary>
	/// <param name="code">The alphabetic code to search by.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The currency if one exists, otherwise <see langword="null"/>.</returns>
	public async Task<CurrencyEntity?> FindByAlphabeticCodeAsync(
		string code,
		CancellationToken cancellationToken = default)
	{
		_logger.FindAlphabeticCode(code);
		var command = new CommandDefinition(_selectAlphabetic, new { code }, cancellationToken: cancellationToken);
		return await _dbConnection.QuerySingleOrDefaultAsync<CurrencyEntity>(command).ConfigureAwait(false);
	}

	/// <summary>Gets a currency with the specified id.</summary>
	/// <param name="id">The id to search by.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The <see cref="CurrencyEntity"/> with the specified id.</returns>
	public async Task<CurrencyEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		_logger.GetId(id);
		var command = new CommandDefinition(_selectId, new { id }, cancellationToken: cancellationToken);
		return await _dbConnection.QuerySingleAsync<CurrencyEntity>(command).ConfigureAwait(false);
	}

	/// <summary>Gets all currencies.</summary>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all currencies.</returns>
	public async Task<List<CurrencyEntity>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		_logger.GetAll();
		var command = new CommandDefinition(Queries.Currency.SelectAll, cancellationToken: cancellationToken);
		var currencies = await _dbConnection.QueryAsync<CurrencyEntity>(command).ConfigureAwait(false);
		return currencies.ToList();
	}
}
