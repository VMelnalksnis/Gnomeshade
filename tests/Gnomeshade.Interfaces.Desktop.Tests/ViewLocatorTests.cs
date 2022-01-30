// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.Desktop.ViewModels;
using Gnomeshade.Interfaces.Desktop.ViewModels.Design;
using Gnomeshade.Interfaces.Desktop.Views;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.Desktop.Tests;

public class ViewLocatorTests
{
	[Test]
	public async Task Build_ShouldReturnExpected()
	{
		var viewLocator = new ViewLocator();
		var viewModel = await AccountViewModel.CreateAsync(new DesignTimeGnomeshadeClient());

		var accountView = viewLocator.Build(viewModel);
		accountView.Should().BeOfType<AccountView>();

		var secondInstanceOfAccountView = viewLocator.Build(viewModel);
		secondInstanceOfAccountView.Should().NotBe(accountView);
	}
}
