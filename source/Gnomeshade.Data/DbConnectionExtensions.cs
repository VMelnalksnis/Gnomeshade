// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Gnomeshade.Data;

/// <summary>Extension methods for managing database connections.</summary>
public static class DbConnectionExtensions
{
	/// <summary>Asynchronously opens the <paramref name="dbConnection"/> if it is not open, and begins a new transaction.</summary>
	/// <param name="dbConnection">The connection to use for beginning the transaction.</param>
	/// <returns>A new database transaction.</returns>
	public static async ValueTask<DbTransaction> OpenAndBeginTransaction(this DbConnection dbConnection)
	{
		if (dbConnection.State.IsNotOpen())
		{
			await dbConnection.OpenAsync();
		}

		return await dbConnection.BeginTransactionAsync();
	}

	private static bool IsNotOpen(this ConnectionState connectionState)
	{
		return (connectionState & ConnectionState.Open) is not ConnectionState.Open;
	}
}
