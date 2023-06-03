// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data.Common;
using System.Threading.Tasks;

using Gnomeshade.Data.Repositories;
using Gnomeshade.Data.Tests.Integration.Fakers;

using Microsoft.Extensions.Logging.Abstractions;

using static Gnomeshade.Data.Tests.Integration.DatabaseInitialization;

namespace Gnomeshade.Data.Tests.Integration.Repositories;

public sealed class CategoryRepositoryTests
{
	private DbConnection _dbConnection = null!;
	private CategoryRepository _repository = null!;

	[SetUp]
	public async Task SetUpAsync()
	{
		_dbConnection = await CreateConnectionAsync().ConfigureAwait(false);
		_repository = new(NullLogger<CategoryRepository>.Instance, _dbConnection);
	}

	[Test]
	public async Task AddGet()
	{
		var tagFaker = new CategoryFaker(TestUser.Id);
		var tag = tagFaker.Generate();
		var childTag = tagFaker.GenerateUnique(tag);

		var tagId = await _repository.AddAsync(tag);

		(await _repository.GetAsync(TestUser.Id))
			.Should()
			.ContainSingle()
			.Which.Id.Should()
			.Be(tagId);

		_ = await _repository.AddAsync(childTag);
	}
}
