// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using AutoMapper;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.V1_0;

/// <summary>
/// Converts all <see cref="DateTimeOffset"/> to UTC.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class DateTimeOffsetUtcConverter : ITypeConverter<DateTimeOffset, DateTimeOffset>
{
	/// <inheritdoc />
	public DateTimeOffset Convert(DateTimeOffset source, DateTimeOffset destination, ResolutionContext context)
	{
		return source.ToUniversalTime();
	}
}
