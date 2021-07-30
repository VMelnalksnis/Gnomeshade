// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.Desktop.ViewModels;

using NUnit.Framework;

namespace Gnomeshade.Interfaces.Desktop.Tests.ViewModels
{
	public class TransactionDetailViewModelTests
	{
		[Test]
		public async Task CanDelete_ShouldBeExpected()
		{
			// todo use actual constructor
			var viewModel = new TransactionDetailViewModel();
			viewModel.Items.Should().HaveCount(2);

			viewModel.SelectedItem.Should().BeNull();
			viewModel.CanDeleteItem.Should().BeFalse();

			viewModel.SelectedItem = viewModel.Items.First();
			viewModel.CanDeleteItem.Should().BeTrue();

			await viewModel.DeleteItemAsync();

			viewModel.SelectedItem.Should().BeNull();
			viewModel.CanDeleteItem.Should().BeFalse();
			viewModel.Items.Should().ContainSingle();
		}

		[Test]
		public void ShouldNotFailAtDesignTime()
		{
			FluentActions
				.Invoking(() => new TransactionDetailViewModel())
				.Should()
				.NotThrow();
		}
	}
}
