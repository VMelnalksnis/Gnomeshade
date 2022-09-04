// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.WebApi.Tests.Integration.Fixtures;

namespace Gnomeshade.WebApi.Tests.Integration;

[SetUpFixture]
public static class WebserverSetup
{
	internal static WebserverFixture WebserverFixture { get; } = new PostgreSQLFixture();

	[OneTimeSetUp]
	public static Task OneTimeSetUpAsync() => WebserverFixture.Initialize();

	[OneTimeTearDown]
	public static Task OneTimeTearDownAsync() => WebserverFixture.DisposeAsync().AsTask();
}
