// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;

using Dapper;

using NodaTime;

namespace Gnomeshade.Data.Dapper;

/// <summary>Type handler for <see cref="Nullable{T}"/> of <see cref="Instant"/>.</summary>
public sealed class NullableInstantTypeHandler : SqlMapper.TypeHandler<Instant?>
{
	/// <inheritdoc />
	public override void SetValue(IDbDataParameter parameter, Instant? value) => parameter.Value = value;

	/// <inheritdoc />
	public override Instant? Parse(object value) => (Instant?)value;
}
