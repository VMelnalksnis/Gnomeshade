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

namespace Gnomeshade.Data.Repositories;

/// <summary>
/// Database backed <see cref="CurrencyRepository"/> repository.
/// </summary>
public sealed class CurrencyRepository : IDisposable
{
	private static readonly string _selectAlphabetic = $"{Queries.Currency.Select} WHERE alphabetic_code = @code;";
	private static readonly string _selectId = $"{Queries.Currency.Select} WHERE id = @id;";

	private readonly DbConnection _dbConnection;

	/// <summary>
	/// Initializes a new instance of the <see cref="CurrencyRepository"/> class with a database connection.
	/// </summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	public CurrencyRepository(DbConnection dbConnection)
	{
		_dbConnection = dbConnection;
	}

	/// <summary>
	/// Searches for a currency with the specified <see cref="CurrencyEntity.AlphabeticCode"/>.
	/// </summary>
	/// <param name="code">The alphabetic code to search by.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The currency if one exists, otherwise <see langword="null"/>.</returns>
	public async Task<CurrencyEntity?> FindByAlphabeticCodeAsync(
		string code,
		CancellationToken cancellationToken = default)
	{
		var command = new CommandDefinition(_selectAlphabetic, new { code }, cancellationToken: cancellationToken);
		return await _dbConnection.QuerySingleOrDefaultAsync<CurrencyEntity>(command).ConfigureAwait(false);
	}

	/// <summary>
	/// Gets a currency with the specified id.
	/// </summary>
	/// <param name="id">The id to search by.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>The <see cref="CurrencyEntity"/> with the specified id.</returns>
	public async Task<CurrencyEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var command = new CommandDefinition(_selectId, new { id }, cancellationToken: cancellationToken);
		return await _dbConnection.QuerySingleAsync<CurrencyEntity>(command).ConfigureAwait(false);
	}

	/// <summary>
	/// Gets all currencies.
	/// </summary>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
	/// <returns>A collection of all currencies.</returns>
	public async Task<List<CurrencyEntity>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		var command = new CommandDefinition(Queries.Currency.Select, cancellationToken: cancellationToken);
		var currencies = await _dbConnection.QueryAsync<CurrencyEntity>(command).ConfigureAwait(false);
		return currencies.ToList();
	}

	/// <inheritdoc />
	public void Dispose() => _dbConnection.Dispose();
}
