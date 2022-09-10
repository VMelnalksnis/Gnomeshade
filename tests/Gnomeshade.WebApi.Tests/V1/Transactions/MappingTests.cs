// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using AutoMapper;

using Gnomeshade.WebApi.V1;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Tests.V1.Transactions;

public class MappingTests
{
	[Test]
	public void AutoMapperTests()
	{
		var mapper = new MapperConfiguration(options =>
		{
			options.CreateMapsForV1_0();
			options.CreateMap<NullableSource, NullableTarget>();
			options.CreateMap<NullableSource, NonNullableTarget>();
		}).CreateMapper();

		var source = new NullableSource { Bytes = default };
		var nullableTarget = mapper.Map<NullableTarget>(source);
		var nonNullableTarget = mapper.Map<NonNullableTarget>(source);

		using (new AssertionScope())
		{
			nullableTarget.Bytes.Should().BeNull();
			nonNullableTarget.Bytes.Should().BeNull("AllowNullCollections does not respect NRT");
		}
	}

	[UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
	private sealed class NullableSource
	{
		public byte[]? Bytes { get; init; }
	}

	private sealed class NullableTarget
	{
		public byte[]? Bytes { get; init; }
	}

	private sealed class NonNullableTarget
	{
		public byte[] Bytes { get; init; } = Array.Empty<byte>();
	}
}
