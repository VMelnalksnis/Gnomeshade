// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gnomeshade.WebApi.Tests.Integration.Fixtures;

[TestFixtureSource(typeof(DatabaseFixtureSource))]
public abstract class WebserverTests
{
	protected WebserverTests(WebserverFixture fixture)
	{
		Fixture = fixture;
	}

	protected WebserverFixture Fixture { get; }

	protected static Task ShouldThrowNotFound(Func<Task> func) =>
		ShouldThrowHttpRequestException(func, HttpStatusCode.NotFound);

	protected static Task ShouldThrowConflict(Func<Task> func) =>
		ShouldThrowHttpRequestException(func, HttpStatusCode.Conflict);

	private static async Task ShouldThrowHttpRequestException(Func<Task> func, HttpStatusCode expected)
	{
		var exceptionAssertions = await FluentActions.Awaiting(func).Should().ThrowExactlyAsync<HttpRequestException>();
		var exception = exceptionAssertions.Which;
		if (exception.StatusCode != expected)
		{
			await TestContext.Out.WriteLineAsync(exception.Message);
		}

		exception.StatusCode.Should().Be(expected);
	}
}
