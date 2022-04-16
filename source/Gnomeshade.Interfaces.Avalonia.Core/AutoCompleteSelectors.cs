// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Interfaces.WebApi.Models.Accounts;
using Gnomeshade.Interfaces.WebApi.Models.Products;

namespace Gnomeshade.Interfaces.Avalonia.Core;

internal static class AutoCompleteSelectors
{
	internal static AutoCompleteSelector<object> Account { get; } = (_, item) => ((Account)item).Name;

	internal static AutoCompleteSelector<object> Counterparty { get; } = (_, item) => ((Counterparty)item).Name;

	internal static AutoCompleteSelector<object> Currency { get; } = (_, item) => ((Currency)item).AlphabeticCode;

	internal static AutoCompleteSelector<object> Product { get; } = (_, item) => ((Product)item).Name;

	internal static AutoCompleteSelector<object> Unit { get; } = (_, item) => ((Unit)item).Name;
}
