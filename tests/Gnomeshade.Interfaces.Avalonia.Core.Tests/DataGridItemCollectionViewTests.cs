// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using Avalonia.Collections;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tests;

public class DataGridItemCollectionViewTests
{
	[Test]
	public void CollectionChanged_ShouldBeRaisedOnItemChanges()
	{
		var firstItem = new Item { Name = "Foo" };
		var otherItem = new Item { Name = "Bar" };
		var items = new List<Item> { firstItem, otherItem };
		var itemCollectionView = new DataGridItemCollectionView<Item>(items);

		var events = new List<(object? Sender, NotifyCollectionChangedEventArgs Args)>();
		itemCollectionView.CollectionChanged += (eventSender, eventArgs) => events.Add((eventSender, eventArgs));

		var dataGridCollectionView = (DataGridCollectionView)itemCollectionView;
		var item = (Item)dataGridCollectionView.GetItemAt(0);
		dataGridCollectionView.EditItem(item);
		item.Name = "Foo 2";
		dataGridCollectionView.CommitEdit();

		var (sender, args) = events.Should().ContainSingle().Subject;
		sender.Should().Be(itemCollectionView);
		args.Action.Should().Be(NotifyCollectionChangedAction.Reset);
		events.Clear();

		dataGridCollectionView.Remove(item);
		(sender, args) = events.Should().ContainSingle().Subject;
		sender.Should().BeEquivalentTo(dataGridCollectionView);
		args.OldItems.Should().NotBeNull();
		args.OldItems!.Cast<Item>().Should().ContainSingle().Which.Should().Be(item);
		events.Clear();

		item.Name = "Foo 3";
		events.Should().BeEmpty();

		var newItem = dataGridCollectionView.AddNew() as Item;
		(sender, args) = events.Should().ContainSingle().Subject;
		sender.Should().BeEquivalentTo(dataGridCollectionView);
		args.NewItems.Should().NotBeNull();
		args.NewItems!.Cast<Item>().Should().ContainSingle().Which.Should().Be(newItem);
	}

	private sealed class Item : PropertyChangedBase
	{
		private string? _name;

		public string? Name
		{
			get => _name;
			set => SetAndNotify(ref _name, value);
		}
	}
}
