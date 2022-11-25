// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc;

[SetUpFixture]
public static class WebserverSetup
{
	internal static readonly KeycloakFixture KeycloakFixture = new();

	[OneTimeSetUp]
	public static async Task OneTimeSetUp()
	{
		await KeycloakFixture.Initialize();
	}

	[OneTimeTearDown]
	public static async Task OneTimeTearDown()
	{
		await KeycloakFixture.DisposeAsync();
	}
}
