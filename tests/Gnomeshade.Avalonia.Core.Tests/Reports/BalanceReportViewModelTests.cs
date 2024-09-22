// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Avalonia.Core.Reports;

using LiveChartsCore.Defaults;

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

			var series = _viewModel.Series.Should().ContainSingle().Subject;
			series
				.Values.Should()
				.HaveCount(14)
				.And.Subject.First().Should()
				.BeEquivalentTo(new FinancialPointI(0, 0, 0, 0));
		}

		_viewModel.SelectedAccounts.Add(_viewModel.UserAccounts.Single(account => account.Name is "Cash"));

		using (new AssertionScope())
		{
			var series = _viewModel.Series.Should().ContainSingle().Subject;
			var point = series.Values.Should().ContainSingle().Subject;

			point.Open.Should().Be(0);
			point.Low.Should().Be(0);
			point.High.Should().Be(127.3);
			point.Close.Should().Be(127.3);
		}

		_viewModel.SelectedAccounts.Clear();
		_viewModel.SelectedAccounts.Add(_viewModel.UserAccounts.Single(account => account.Name is "Spending"));

		using (new AssertionScope())
		{
			var series = _viewModel.Series.Should().ContainSingle().Subject;
			series
				.Values.Should()
				.HaveCount(14)
				.And.Subject.First().Should()
				.BeEquivalentTo(new FinancialPointI(0, 0, -127.3, -127.3));
		}
	}
}
