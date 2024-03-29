﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Counterparties;
using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.WebApi.Client;

namespace Gnomeshade.Avalonia.Core.Tests.Counterparties;

[TestOf(typeof(CounterpartyViewModel))]
public sealed class CounterpartyViewModelTests
{
	private IGnomeshadeClient _gnomeshadeClient = null!;
	private CounterpartyViewModel _viewModel = null!;

	[SetUp]
	public async Task SetUp()
	{
		_gnomeshadeClient = new DesignTimeGnomeshadeClient();
		_viewModel = new(new StubbedActivityService(), _gnomeshadeClient);
		await _viewModel.RefreshAsync();
	}

	[Test]
	public async Task ShouldContainUserCounterparty()
	{
		var userCounterparty = await _gnomeshadeClient.GetMyCounterpartyAsync();

		_viewModel.Rows.Should().ContainSingle(row => row.Id == userCounterparty.Id);
	}
}
