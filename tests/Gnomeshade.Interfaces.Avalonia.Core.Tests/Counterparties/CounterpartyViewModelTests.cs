// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.Avalonia.Core.Counterparties;
using Gnomeshade.Interfaces.Avalonia.Core.DesignTime;
using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests.Counterparties;

[TestOf(typeof(CounterpartyViewModel))]
public sealed class CounterpartyViewModelTests
{
	private IGnomeshadeClient _gnomeshadeClient = null!;
	private CounterpartyViewModel _viewModel = null!;

	[SetUp]
	public async Task SetUp()
	{
		_gnomeshadeClient = new DesignTimeGnomeshadeClient();
		_viewModel = await CounterpartyViewModel.CreateAsync(_gnomeshadeClient);
	}

	[Test]
	public async Task LoanBalance_UserBalanceShouldBeZero()
	{
		var userCounterparty = await _gnomeshadeClient.GetMyCounterpartyAsync();

		_viewModel.Counterparties.Single(row => row.Id == userCounterparty.Id).LoanBalance.Should().Be(0);
	}

	[Test]
	public async Task LoanBalance_ShouldBeExpected()
	{
		var userCounterparty = await _gnomeshadeClient.GetMyCounterpartyAsync();

		_viewModel.Counterparties.Single(row => row.Id != userCounterparty.Id).LoanBalance.Should().Be(-95);
	}
}
