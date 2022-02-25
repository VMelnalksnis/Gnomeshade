// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Execution;

using Gnomeshade.Interfaces.Avalonia.Core.Accounts;
using Gnomeshade.Interfaces.Avalonia.Core.DesignTime;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests.Accounts;

public class AccountDetailViewModelTests
{
	private IGnomeshadeClient _gnomeshadeClient = null!;
	private Account _account = null!;

	[OneTimeSetUp]
	public async Task OneTimeSetUp()
	{
		_gnomeshadeClient = new DesignTimeGnomeshadeClient();
		_account = await _gnomeshadeClient.GetAccountAsync(Guid.Empty);
	}

	[Test]
	public async Task CreateAsync_ShouldHaveExpectedInitialValues()
	{
		var viewModel = await AccountDetailViewModel.CreateAsync(_gnomeshadeClient, _account.Id);

		using (new AssertionScope())
		{
			viewModel.Name.Should().Be(_account.Name);
			viewModel.Bic.Should().Be(_account.Bic);
			viewModel.Iban.Should().Be(_account.Iban);
			viewModel.AccountNumber.Should().Be(_account.AccountNumber);
			viewModel.PreferredCurrency.Should().Be(_account.PreferredCurrency);

			viewModel.Currencies.Should().HaveCount(2);
			viewModel.CanUpdate.Should().BeTrue();
		}
	}

	[Test]
	public async Task UpdateAccountAsync_ShouldPutAccount()
	{
		var viewModel = await AccountDetailViewModel.CreateAsync(_gnomeshadeClient, _account.Id);
		viewModel.Bic = $"{viewModel.Bic}123";

		await FluentActions
			.Awaiting(() => viewModel.UpdateAccountAsync())
			.Should()
			.ThrowExactlyAsync<NotImplementedException>($"{nameof(DesignTimeGnomeshadeClient)} does not implement {nameof(IGnomeshadeClient.PutAccountAsync)}");
	}
}
