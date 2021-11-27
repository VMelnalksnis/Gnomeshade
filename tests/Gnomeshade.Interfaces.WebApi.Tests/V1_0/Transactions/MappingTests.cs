// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using AutoMapper;

using FluentAssertions;
using FluentAssertions.Execution;

using Gnomeshade.Data.Entities;
using Gnomeshade.Interfaces.WebApi.Models.Transactions;
using Gnomeshade.Interfaces.WebApi.V1_0;

using JetBrains.Annotations;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.WebApi.Tests.V1_0.Transactions;

public class MappingTests
{
	private IMapper _mapper = null!;

	[OneTimeSetUp]
	public void OneTimeSetUp()
	{
		var configuration = new MapperConfiguration(options => options.CreateMapsForV1_0());
		_mapper = configuration.CreateMapper();
	}

	[Test]
	public void CreationToDatabase()
	{
		var creationModel = new TransactionCreationModel();

		var transaction = _mapper.Map<TransactionEntity>(creationModel);

		transaction.ImportHash.Should().BeNull();
	}

	[Test]
	public void AutoMapperTests()
	{
		var mapper = new MapperConfiguration(options =>
		{
			options.CreateMap<NullableSource, NullableTarget>();
			options.CreateMap<NullableSource, NonNullableTarget>();
			options.AllowNullCollections = true;
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
