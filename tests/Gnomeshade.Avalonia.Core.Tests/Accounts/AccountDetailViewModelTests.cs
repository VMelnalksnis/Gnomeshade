// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Accounts;
using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Interfaces.WebApi.Client;
using Gnomeshade.Interfaces.WebApi.Models.Accounts;

namespace Gnomeshade.Avalonia.Core.Tests.Accounts;

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
		var viewModel = new AccountUpsertionViewModel(_gnomeshadeClient, _account.Id);
		await viewModel.RefreshAsync();

		using (new AssertionScope())
		{
			viewModel.Name.Should().Be(_account.Name);
			viewModel.Bic.Should().Be(_account.Bic);
			viewModel.Iban.Should().Be(_account.Iban);
			viewModel.AccountNumber.Should().Be(_account.AccountNumber);
			viewModel.PreferredCurrency.Should().Be(_account.PreferredCurrency);

			viewModel.Currencies.Should().HaveCount(2);
			viewModel.CanSave.Should().BeTrue();
		}
	}

	[Test]
	public async Task UpdateAccountAsync_ShouldPutAccount()
	{
		var viewModel = new AccountUpsertionViewModel(_gnomeshadeClient, _account.Id);
		await viewModel.RefreshAsync();
		viewModel.Bic = $"{viewModel.Bic}123";

		await FluentActions
			.Awaiting(() => viewModel.SaveAsync())
			.Should()
			.NotThrowAsync();
		viewModel.ErrorMessage.Should().NotBeNullOrWhiteSpace();
	}
}
