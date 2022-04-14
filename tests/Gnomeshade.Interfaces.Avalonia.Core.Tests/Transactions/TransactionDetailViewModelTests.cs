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
	[Test]
	public async Task SaveAsync_ShouldUpdate()
	{
		var viewModel = await CreateAsync(new DesignTimeGnomeshadeClient(), Guid.Empty);

		var reconciledAt = DateTime.Now;

		viewModel.Properties.ReconciliationDate = reconciledAt;
		viewModel.Properties.ReconciliationTime = reconciledAt.TimeOfDay;

		viewModel.CanSave.Should().BeTrue();

		await viewModel.SaveAsync();

		viewModel.ErrorMessage.Should().BeNullOrWhiteSpace();
		viewModel.Properties.ReconciledAt.Should().BeEquivalentTo(
			new ZonedDateTime(
				Instant.FromUtc(reconciledAt.Year, reconciledAt.Month, reconciledAt.Day, reconciledAt.Hour, reconciledAt.Minute),
				DateTimeZoneProviders.Tzdb.GetSystemDefault()));
	}
}
