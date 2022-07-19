// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.Interfaces.Avalonia.Core.DesignTime;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Purchases;

using NodaTime;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests.Transactions.Purchases;

[TestOf(typeof(PurchaseViewModel))]
public class PurchaseViewModelTests
{
	private PurchaseViewModel _viewModel = null!;

	[SetUp]
	public async Task SetUp()
	{
		_viewModel = new(new DesignTimeGnomeshadeClient(), DateTimeZoneProviders.Tzdb, Guid.Empty);
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
		_viewModel.Details.ErrorMessage.Should().BeNullOrWhiteSpace();
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
