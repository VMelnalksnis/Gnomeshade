// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;

using Dapper;

namespace Gnomeshade.Data.PostgreSQL.Dapper;

internal sealed class UnsignedIntegerTypeHandler : SqlMapper.TypeHandler<uint?>
{
	/// <inheritdoc />
	public override void SetValue(IDbDataParameter parameter, uint? value)
	{
		parameter.DbType = DbType.Int32;
		parameter.Value = (int?)value;
	}

	/// <inheritdoc />
	public override uint? Parse(object? value) => value is int number ? (uint)number : null;
}
