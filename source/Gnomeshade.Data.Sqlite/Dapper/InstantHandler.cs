// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data;

using Dapper;

using NodaTime;
using NodaTime.Text;

namespace Gnomeshade.Data.Sqlite.Dapper;

internal sealed class InstantHandler : SqlMapper.TypeHandler<Instant>
{
	internal static readonly InstantPattern Pattern = InstantPattern.CreateWithInvariantCulture(_sqliteDatetimeFormat);
	private const string _sqliteDatetimeFormat = "uuuu'-'MM'-'dd' 'HH:mm:ss.FFFFFFFFF";

	/// <inheritdoc/>
	public override void SetValue(IDbDataParameter parameter, Instant value) => parameter.Value = Pattern.Format(value);

	/// <inheritdoc/>
	public override Instant Parse(object value) => Pattern.Parse((string)value).GetValueOrThrow();
}
