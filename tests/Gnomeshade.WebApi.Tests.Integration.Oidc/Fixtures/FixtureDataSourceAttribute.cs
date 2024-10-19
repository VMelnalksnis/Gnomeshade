// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc.Fixtures;

public sealed class FixtureDataSourceAttribute : DataSourceGeneratorAttribute<KeycloakFixture>
{
	private static readonly KeycloakFixture[] _fixtures =
	[
		new(5001, 8297),
		new(5002, 8298),
	];

	[Before(TestSession)]
	public static async Task BeforeAssembly()
	{
		await Task.WhenAll(_fixtures.Select(fixture => fixture.InitializeAsync()));
	}

	[After(TestSession)]
	public static async Task AfterAssembly()
	{
		await Task.WhenAll(_fixtures.Select(fixture => fixture.DisposeAsync().AsTask()));
	}

	/// <inheritdoc />
	public override IEnumerable<KeycloakFixture> GenerateDataSources(DataGeneratorMetadata dataGeneratorMetadata)
	{
		return _fixtures;
	}
}
