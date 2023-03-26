// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Repositories;

using Microsoft.Extensions.Logging.Abstractions;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.Repositories;

public static class EntityFactory
{
	private static List<CurrencyEntity>? _currencies;

	public static async Task<List<CurrencyEntity>> GetCurrenciesAsync()
	{
		if (_currencies is not null)
		{
			return _currencies.ToList();
		}

		var dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
		_currencies = await new CurrencyRepository(NullLogger<CurrencyRepository>.Instance, dbConnection).GetAllAsync().ConfigureAwait(false);
		return _currencies;
	}
}
