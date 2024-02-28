// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Avalonia.Core.Reports;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Tests.Reports;

[TestOf(typeof(BalanceReportViewModel))]
public sealed class BalanceReportViewModelTests
{
	private BalanceReportViewModel _viewModel = null!;

	[SetUp]
	public async Task SetUp()
	{
		_viewModel = new(
			new StubbedActivityService(),
			new DesignTimeGnomeshadeClient(),
			SystemClock.Instance,
			DateTimeZoneProviders.Tzdb);

		await _viewModel.RefreshAsync();
	}

	[Test]
	public void ShouldDisplayCorrectData()
	{
		using (new AssertionScope())
		{
			_viewModel.UserAccounts.Should().HaveCount(2);
			_viewModel.SelectedAccounts.Should().BeEmpty();

			_viewModel.Currencies.Should().HaveCount(2);
			_viewModel.SelectedCurrency.Should().BeNull();

			var point = _viewModel.Series.Should().ContainSingle().Which.Values.Should().ContainSingle().Subject;

			point.Open.Should().Be(0);
			point.Low.Should().Be(0);
			point.High.Should().Be(0);
			point.Close.Should().Be(0);
		}

		_viewModel.SelectedAccounts.Add(_viewModel.UserAccounts.First());

		using (new AssertionScope())
		{
			var point = _viewModel.Series.Should().ContainSingle().Which.Values.Should().ContainSingle().Subject;

			point.Open.Should().Be(0);
			point.Low.Should().Be(0);
			point.High.Should().Be(127.3);
			point.Close.Should().Be(127.3);
		}

		_viewModel.SelectedAccounts.Clear();
		_viewModel.SelectedAccounts.Add(_viewModel.UserAccounts.Last());

		using (new AssertionScope())
		{
			var point = _viewModel.Series.Should().ContainSingle().Which.Values.Should().ContainSingle().Subject;

			point.Open.Should().Be(0);
			point.Low.Should().Be(-127.3);
			point.High.Should().Be(0);
			point.Close.Should().Be(-127.3);
		}
	}
}
