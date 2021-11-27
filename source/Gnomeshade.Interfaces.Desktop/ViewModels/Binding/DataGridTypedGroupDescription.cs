// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Globalization;
using System.Linq.Expressions;

using Avalonia.Collections;

namespace Gnomeshade.Interfaces.Desktop.ViewModels.Binding;

/// <summary>
/// A strongly typed version of <see cref="DataGridPathGroupDescription"/>.
/// </summary>
/// <typeparam name="TItem">The type of the object that is being groped.</typeparam>
/// <typeparam name="TKey">The type of value by which to group items.</typeparam>
public class DataGridTypedGroupDescription<TItem, TKey> : DataGridGroupDescription
	where TKey : class
{
	private readonly Func<TItem, TKey> _keySelector;

	/// <summary>
	/// Initializes a new instance of the <see cref="DataGridTypedGroupDescription{TItem, TKey}"/> class.
	/// </summary>
	/// <param name="keySelector">Function for getting the value by which to group items.</param>
	public DataGridTypedGroupDescription(Expression<Func<TItem, TKey>> keySelector)
	{
		_keySelector = keySelector.Compile();
		PropertyName =
			keySelector.Body.NodeType == ExpressionType.MemberAccess
				? ((MemberExpression)keySelector.Body).Member.Name
				: string.Empty;
	}

	/// <inheritdoc />
	public override string PropertyName { get; }

	/// <inheritdoc />
	public override object? GroupKeyFromItem(object item, int level, CultureInfo culture)
	{
		return item is not TItem typedItem
			? null
			: _keySelector(typedItem);
	}

	/// <inheritdoc />
	public override bool KeysMatch(object? groupKey, object? itemKey)
	{
		if (groupKey is TKey typedGroupKey && itemKey is TKey typedItemKey)
		{
			return typedGroupKey.Equals(typedItemKey);
		}

		return base.KeysMatch(groupKey, itemKey);
	}
}
