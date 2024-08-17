// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Globalization;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Data;

using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Avalonia.Core.Transactions.Purchases;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Tests;

[TestOf(typeof(NodaTimeValueConverter<>))]
public sealed class NodaTimeValueConverterTests
{
	private PurchaseUpsertionViewModel _viewModel = null!;
	private TextBlock _deliveryTextBlock = null!;

	[SetUp]
	public async Task SetUp()
	{
		var activityService = new StubbedActivityService();
		_viewModel = new(
			activityService,
			new DesignTimeGnomeshadeClient(),
			new DesignTimeDialogService(),
			DateTimeZoneProviders.Tzdb,
			Guid.Empty,
			null);

		await _viewModel.RefreshAsync();

		var binding = new Binding
		{
			Source = _viewModel,
			Path = nameof(_viewModel.DeliveryDate),
			Converter = new LocalDateTimeConverter(),
			Mode = BindingMode.TwoWay,
			ConverterCulture = CultureInfo.InvariantCulture,
		};

		_deliveryTextBlock = new();
		_deliveryTextBlock.Bind(TextBlock.TextProperty, binding);
	}

	[Test]
	public void Convert_TextShouldBeEmptyForNull()
	{
		_viewModel.DeliveryDate = null;

		_deliveryTextBlock.Text.Should().BeNullOrEmpty();
	}

	[Test]
	public void Convert_TextShouldBeExpectedForValue()
	{
		_viewModel.DeliveryDate = new LocalDateTime(2024, 08, 09, 07, 55, 02);

		_deliveryTextBlock.Text.Should().Be("08/09/2024 07:55");
	}

	[Test]
	public void ConvertBack_ShouldSetToNull()
	{
		_viewModel.DeliveryDate = LocalDateTime.MaxIsoValue;

		_deliveryTextBlock.Text.Should().NotBeNullOrWhiteSpace();

		_deliveryTextBlock.Text = string.Empty;

		_viewModel.DeliveryDate.Should().BeNull();
	}

	[Test]
	public void ConvertBack_Invalid()
	{
		_viewModel.DeliveryDate = null;

		_deliveryTextBlock.Text = "Foo";

		_viewModel.DeliveryDate.Should().BeNull();
	}
}
