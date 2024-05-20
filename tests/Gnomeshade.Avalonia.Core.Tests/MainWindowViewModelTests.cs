// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Configuration;
using Gnomeshade.Avalonia.Core.Counterparties;
using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Avalonia.Core.Products;
using Gnomeshade.Avalonia.Core.Transactions;

namespace Gnomeshade.Avalonia.Core.Tests;

[TestOf(typeof(MainWindowViewModel))]
public sealed class MainWindowViewModelTests
{
	[Test]
	public async Task NavigateBack()
	{
		var viewModel = DesignTimeData.MainWindowViewModel;

		viewModel.Initialize.Execute(null);
		while (viewModel.IsBusy)
		{
			await Task.Delay(100);
		}

		using (new AssertionScope())
		{
			viewModel.ActiveView.Should().BeOfType<ConfigurationWizardViewModel>("configuration should not be valid");
			viewModel.NavigateBack.CanExecute(null).Should().BeFalse();
		}

		await viewModel.SwitchToTransactionOverviewAsync();
		using (new AssertionScope())
		{
			viewModel.ActiveView.Should().BeOfType<TransactionViewModel>();
			viewModel.NavigateBack.CanExecute(null).Should().BeFalse();
		}

		await viewModel.SwitchToCategoriesAsync();
		using (new AssertionScope())
		{
			viewModel.ActiveView.Should().BeOfType<CategoryViewModel>();
			viewModel.NavigateBack.CanExecute(null).Should().BeTrue();
		}

		await viewModel.SwitchToCounterpartiesAsync();
		using (new AssertionScope())
		{
			viewModel.ActiveView.Should().BeOfType<CounterpartyViewModel>();
			viewModel.NavigateBack.CanExecute(null).Should().BeTrue();
		}

		viewModel.NavigateBack.Execute(null);
		while (viewModel.IsBusy)
		{
			await Task.Delay(100);
		}

		using (new AssertionScope())
		{
			viewModel.ActiveView.Should().BeOfType<CategoryViewModel>();
			viewModel.NavigateBack.CanExecute(null).Should().BeTrue();
		}

		viewModel.NavigateBack.Execute(null);
		while (viewModel.IsBusy)
		{
			await Task.Delay(100);
		}

		using (new AssertionScope())
		{
			viewModel.NavigateBack.CanExecute(null).Should().BeFalse();
			viewModel.ActiveView.Should().BeOfType<TransactionViewModel>();
		}
	}
}
