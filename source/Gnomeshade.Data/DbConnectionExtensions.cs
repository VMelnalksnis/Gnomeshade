// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;

namespace Gnomeshade.Data
{
	internal static class DbConnectionExtensions
	{
		internal static IDbTransaction OpenAndBeginTransaction(this IDbConnection dbConnection)
		{
			if (!dbConnection.State.HasFlag(ConnectionState.Open))
			{
				dbConnection.Open();
			}

			return dbConnection.BeginTransaction();
		}
	}
}
