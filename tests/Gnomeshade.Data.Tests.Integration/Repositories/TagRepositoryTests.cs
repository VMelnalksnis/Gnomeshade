// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Data.Repositories;
using Gnomeshade.TestingHelpers.Data.Fakers;

using NUnit.Framework;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.Repositories;

public class TagRepositoryTests : IDisposable
{
	private IDbConnection _dbConnection = null!;
	private TagRepository _repository = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
		_repository = new(_dbConnection);
	}

	[TearDown]
	public void Dispose()
	{
		_dbConnection.Dispose();
		_repository.Dispose();
	}

	[Test]
	public async Task AddGet()
	{
		var tagFaker = new TagFaker(TestUser.Id);
		var tag = tagFaker.Generate();
		var childTag = tagFaker.GenerateUnique(tag);

		var tagId = await _repository.AddAsync(tag);

		(await _repository.GetAllAsync(TestUser.Id))
			.Should()
			.ContainSingle()
			.Which.Id.Should()
			.Be(tagId);

		var childTagId = await _repository.AddAsync(childTag);
		await _repository.TagAsync(tagId, childTagId, TestUser.Id);

		(await _repository.GetTaggedAsync(childTagId, TestUser.Id))
			.Should()
			.ContainSingle()
			.Which.Id.Should()
			.Be(tagId);

		await _repository.UntagAsync(tagId, childTagId, TestUser.Id);
		(await _repository.GetTaggedAsync(childTagId, TestUser.Id))
			.Should()
			.BeEmpty();
	}
}
