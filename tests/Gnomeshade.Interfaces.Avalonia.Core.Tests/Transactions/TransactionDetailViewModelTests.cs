// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.Avalonia.Core.DesignTime;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions;

using NodaTime;

using NUnit.Framework;

using static Gnomeshade.Interfaces.Avalonia.Core.Transactions.TransactionUpsertionViewModel;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests.Transactions;

[TestOf(typeof(TransactionUpsertionViewModel))]
public class TransactionDetailViewModelTests
{
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider = DateTimeZoneProviders.Tzdb;

	[Test]
	public async Task SaveAsync_ShouldUpdate()
	{
		var viewModel = await CreateAsync(new DesignTimeGnomeshadeClient(), Guid.Empty, _dateTimeZoneProvider);

		var reconciledAt = new DateTimeOffset(2022, 04, 15, 12, 50, 30, TimeSpan.FromHours(3));

		viewModel.Properties.ReconciliationDate = reconciledAt;
		viewModel.Properties.ReconciliationTime = reconciledAt.ToLocalTime().TimeOfDay;

		viewModel.CanSave.Should().BeTrue();

		await viewModel.SaveAsync();

		viewModel.ErrorMessage.Should().BeNullOrWhiteSpace();
		viewModel.Properties.ReconciledAt.Should().BeEquivalentTo(
			new ZonedDateTime(
				Instant.FromUtc(2022, 04, 15, 9, 50, 0),
				_dateTimeZoneProvider.GetSystemDefault()));
	}
}
