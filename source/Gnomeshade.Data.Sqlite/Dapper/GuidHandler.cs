// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;

using Dapper;

namespace Gnomeshade.Data.Sqlite.Dapper;

internal sealed class GuidHandler : SqlMapper.TypeHandler<Guid>
{
	/// <inheritdoc />
	public override void SetValue(IDbDataParameter parameter, Guid value) => parameter.Value = value.ToString();

	/// <inheritdoc />
	public override Guid Parse(object value) => new((string)value);
}
