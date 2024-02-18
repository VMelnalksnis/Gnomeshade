// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core.Reports.Aggregates;
using Gnomeshade.Avalonia.Core.Reports.Calculations;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Loans;
using Gnomeshade.WebApi.Models.Owners;
using Gnomeshade.WebApi.Models.Products;

namespace Gnomeshade.Avalonia.Core;

internal static class AutoCompleteSelectors
{
	internal static AutoCompleteSelector<object> Access { get; } = (_, item) => ((Access)item).Name;

	internal static AutoCompleteSelector<object> Account { get; } = (_, item) => ((Account)item).Name;

	/// <summary>Gets a delegate for formatting a counterparty in an <see cref="AutoCompleteBox"/>.</summary>
	internal static AutoCompleteSelector<object> Counterparty { get; } = (_, item) => ((Counterparty)item).Name;

	internal static AutoCompleteSelector<object> Category { get; } = (_, item) => ((Category)item).Name;

	/// <summary>Gets a delegate for formatting a currency in an <see cref="AutoCompleteBox"/>.</summary>
	internal static AutoCompleteSelector<object> Currency { get; } = (_, item) => ((Currency)item).AlphabeticCode;

	/// <summary>Gets a delegate for formatting a owner in an <see cref="AutoCompleteBox"/>.</summary>
	internal static AutoCompleteSelector<object> Owner { get; } = (_, item) => ((Owner)item).Name;

	internal static AutoCompleteSelector<object> Product { get; } = (_, item) => ((Product)item).Name;

	internal static AutoCompleteSelector<object> Unit { get; } = (_, item) => ((Unit)item).Name;

	internal static AutoCompleteSelector<object> Aggregate { get; } = (_, item) => ((IAggregateFunction)item).Name;

	internal static AutoCompleteSelector<object> Calculation { get; } = (_, item) => ((ICalculationFunction)item).Name;

	/// <summary>Gets a delegate for formatting a loan in an <see cref="AutoCompleteBox"/>.</summary>
	internal static AutoCompleteSelector<object> Loan { get; } = (_, item) => ((Loan)item).Name;
}
