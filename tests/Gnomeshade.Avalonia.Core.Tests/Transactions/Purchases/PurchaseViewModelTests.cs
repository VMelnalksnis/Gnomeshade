// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Avalonia.Core.Transactions.Purchases;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Tests.Transactions.Purchases;

[TestOf(typeof(PurchaseViewModel))]
public class PurchaseViewModelTests
{
	private PurchaseViewModel _viewModel = null!;

	[SetUp]
	public async Task SetUp()
	{
		var activityService = new StubbedActivityService();
		_viewModel = new(activityService, new DesignTimeGnomeshadeClient(), new DesignTimeDialogService(), DateTimeZoneProviders.Tzdb, Guid.Empty);
		await _viewModel.RefreshAsync();
	}

	[Test]
	public async Task SelectingRow_ShouldUpdateDetails()
	{
		const decimal totalTransferred = 123.95m;
		const decimal price = 10;

		using (new AssertionScope())
		{
			_viewModel.Selected.Should().BeNull();
			_viewModel.Details.Should().NotBeNull();
			_viewModel.Details.CanSave.Should().BeFalse();
			_viewModel.Details.Order.Should().Be(1);
		}

		_viewModel.Selected = _viewModel.Rows.First();
		await _viewModel.UpdateSelection();
		_viewModel.Details.Should().NotBeNull();
		_viewModel.Details.CanSave.Should().BeTrue();

		_viewModel.Selected = null;
		await _viewModel.UpdateSelection();
		_viewModel.Details.CanSave.Should().BeFalse();
		_viewModel.Details.Currency?.AlphabeticCode.Should().Be("EUR");
		_viewModel.Details.Price.Should().Be(totalTransferred);

		_viewModel.Details.Price = price;
		_viewModel.Details.Product = _viewModel.Details.Products.First();
		_viewModel.Details.Amount = 1;
		await _viewModel.Details.SaveAsync();
		await _viewModel.RefreshAsync();

		_viewModel.Selected = _viewModel.Rows.First();
		await _viewModel.UpdateSelection();
		_viewModel.Selected = null;
		await _viewModel.UpdateSelection();

		_viewModel.Details.CanSave.Should().BeFalse();
		_viewModel.Details.Currency?.AlphabeticCode.Should().Be("EUR");
		_viewModel.Details.Price.Should().Be(totalTransferred - price);
	}
}
