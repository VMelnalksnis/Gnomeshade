// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.Desktop.ViewModels;
using Gnomeshade.Interfaces.Desktop.ViewModels.Design;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.Desktop.Tests.ViewModels;

public class CounterpartyMergeViewModelTests
{
	private CounterpartyMergeViewModel _viewModel = null!;

	[SetUp]
	public async Task SetUp()
	{
		_viewModel = await CounterpartyMergeViewModel.CreateAsync(new DesignTimeGnomeshadeClient());
	}

	[Test]
	public void CanMerge_NoCounterpartiesSelected()
	{
		_viewModel.CanMerge.Should().BeFalse();
	}

	[Test]
	public void CanMerge_OnlySourceSelected()
	{
		_viewModel.SourceCounterparty = _viewModel.SourceCounterparties.First();
		_viewModel.CanMerge.Should().BeFalse();
	}

	[Test]
	public void CanMerge_OnlyTargetSelected()
	{
		_viewModel.TargetCounterparty = _viewModel.TargetCounterparties.First();
		_viewModel.CanMerge.Should().BeFalse();
	}

	[Test]
	public void CanMerge_SourceAndTargetIsTheSame()
	{
		_viewModel.SourceCounterparty = _viewModel.SourceCounterparties.First();
		_viewModel.TargetCounterparty = _viewModel.TargetCounterparties.Single(row => row.Id == _viewModel.SourceCounterparty.Id);
		_viewModel.CanMerge.Should().BeFalse();
	}
}
