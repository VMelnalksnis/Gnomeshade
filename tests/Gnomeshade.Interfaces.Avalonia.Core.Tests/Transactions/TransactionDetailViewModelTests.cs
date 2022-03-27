// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Interfaces.Avalonia.Core.DesignTime;
using Gnomeshade.Interfaces.Avalonia.Core.Transactions.Items;

using NUnit.Framework;

using static Gnomeshade.Interfaces.Avalonia.Core.Transactions.Items.TransactionDetailViewModel;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests.Transactions;

[TestOf(typeof(TransactionDetailViewModel))]
public class TransactionDetailViewModelTests
{
	[Test]
	public async Task CanDelete_ShouldBeExpected()
	{
		var viewModel = await CreateAsync(new DesignTimeGnomeshadeClient(), Guid.Empty);
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
	public async Task AddItemAsync_ShouldRefreshDataGridView()
	{
		var changedProperties = new List<PropertyChangedEventArgs>();
		var viewModel = await CreateAsync(new DesignTimeGnomeshadeClient(), Guid.Empty);
		viewModel.PropertyChanged += (_, args) => changedProperties.Add(args);

		var existingItem = viewModel.Items.First();
		var itemCreation = viewModel.ItemCreation;
		itemCreation.SourceAccount = itemCreation.Accounts.Single(a => a.Name == existingItem.SourceAccount);
		itemCreation.SourceAmount = existingItem.SourceAmount;
		itemCreation.TargetAccount = itemCreation.Accounts.Single(a => a.Name == existingItem.TargetAccount);
		itemCreation.Product = itemCreation.Products.Single(p => p.Name == existingItem.Product);
		itemCreation.Amount = existingItem.Amount;

		viewModel.CanAddItem.Should().BeTrue();
		await viewModel.AddItemAsync();

		changedProperties.Should().Contain(args => args.PropertyName == nameof(TransactionDetailViewModel.DataGridView));

		viewModel.SelectedItem = viewModel.Items.Last();
		await viewModel.DeleteItemAsync();
	}

	[Test]
	public async Task SetSelectedItem_ShouldUpdateItemCreation()
	{
		var viewModel = await CreateAsync(new DesignTimeGnomeshadeClient(), Guid.Empty);
		var itemToSelect = viewModel.Items.First();

		viewModel.ItemCreation.Amount.Should().BeNull();

		viewModel.SelectedItem = itemToSelect;

		viewModel.ItemCreation.Amount.Should().Be(itemToSelect.Amount);
		viewModel.ItemCreation.Amount.Should().NotBeNull();
	}
}
