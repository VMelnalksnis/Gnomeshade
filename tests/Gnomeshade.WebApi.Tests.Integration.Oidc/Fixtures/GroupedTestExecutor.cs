// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using TUnit.Core.Interfaces;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc.Fixtures;

public abstract class GroupedTestExecutor<TFixture> : ITestExecutor
	where TFixture : notnull
{
	private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
	private static readonly ConcurrentDictionary<TFixture, bool> _initialized = [];

	protected static ConcurrentDictionary<TFixture, List<TestContext>> Tests { get; set; } = [];

	/// <inheritdoc />
	public async Task ExecuteTest(TestContext context, Func<Task> action)
	{
		if (!TryGetKey(context, out var fixture))
		{
			await action();
			return;
		}

		if (!Tests.TryGetValue(fixture, out var tests))
		{
			throw new();
		}

		lock (fixture)
		{
			if (_initialized.TryAdd(fixture, false))
			{
				_semaphoreSlim.Wait();
				BeforeFirstTest(fixture);
			}
		}

		await action();

		tests.Remove(context);
		if (tests.Count is 0)
		{
			_semaphoreSlim.Release();
		}
	}

	protected abstract bool TryGetKey(TestContext context, [MaybeNullWhen(false)] out TFixture key);

	protected virtual void BeforeFirstTest(TFixture fixture)
	{
	}
}
