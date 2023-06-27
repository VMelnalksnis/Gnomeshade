// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Avalonia.Core.Transactions;
using Gnomeshade.TestingHelpers.Models;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Tests.Transactions;

[TestOf(typeof(TransactionUpsertionViewModel))]
public sealed class TransactionDetailViewModelTests
{
	private readonly IClock _clock = SystemClock.Instance;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider = DateTimeZoneProviders.Tzdb;

	[Test]
	public async Task SaveAsync_ShouldUpdate()
	{
		var client = new DesignTimeGnomeshadeClient();
		var viewModel = new TransactionUpsertionViewModel(new StubbedActivityService(), client, new DesignTimeDialogService(), _clock, _dateTimeZoneProvider, Guid.Empty);
		await viewModel.RefreshAsync();

		var instant = Instant.FromUtc(2022, 04, 15, 9, 50, 30);
		var reconciledAt = instant.InZone(_dateTimeZoneProvider.GetSystemDefault()).LocalDateTime;

		viewModel.Properties.ReconciliationDate = reconciledAt;

		viewModel.CanSave.Should().BeTrue();

		await viewModel.SaveAsync();

		viewModel.Properties.ReconciledAt.Should().BeEquivalentTo(new ZonedDateTime(instant, _dateTimeZoneProvider.GetSystemDefault()));

		var transaction = await client.GetTransactionAsync(Guid.Empty);
		var creation = transaction.ToCreation() with { ReconciledAt = null };
		await client.PutTransactionAsync(Guid.Empty, creation);
	}
}
