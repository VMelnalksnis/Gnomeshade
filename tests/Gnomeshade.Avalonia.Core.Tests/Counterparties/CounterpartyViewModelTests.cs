// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Counterparties;
using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Interfaces.WebApi.Client;

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
		_viewModel = new(_gnomeshadeClient);
		await _viewModel.RefreshAsync();
	}

	[Test]
	public async Task LoanBalance_UserBalanceShouldBeZero()
	{
		var userCounterparty = await _gnomeshadeClient.GetMyCounterpartyAsync();

		_viewModel.Rows.Single(row => row.Id == userCounterparty.Id).LoanBalance.Should().Be(0);
	}

	[Test]
	public async Task LoanBalance_ShouldBeExpected()
	{
		var userCounterparty = await _gnomeshadeClient.GetMyCounterpartyAsync();

		_viewModel.Rows.Single(row => row.Id != userCounterparty.Id).LoanBalance.Should().Be(-95);
	}
}
