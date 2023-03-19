// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;

using Dapper;

namespace Gnomeshade.Data.Sqlite.Dapper;

internal sealed class DecimalHandler : SqlMapper.TypeHandler<decimal>
{
	/// <inheritdoc />
	public override void SetValue(IDbDataParameter parameter, decimal value) => parameter.Value = value;

	/// <inheritdoc />
	public override decimal Parse(object value) => Convert.ToDecimal(value);
}
