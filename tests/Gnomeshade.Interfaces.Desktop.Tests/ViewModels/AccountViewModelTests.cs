// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.Desktop.ViewModels;
using Gnomeshade.Interfaces.Desktop.ViewModels.Design;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.Desktop.Tests.ViewModels
{
	public class AccountViewModelTests
	{
		[Test]
		public async Task DataGridView_ShouldBeGrouped()
		{
			var viewModel = await AccountViewModel.CreateAsync(new DesignTimeGnomeshadeClient());

			viewModel.DataGridView.Groups.Should().HaveCount(2);
		}
	}
}
