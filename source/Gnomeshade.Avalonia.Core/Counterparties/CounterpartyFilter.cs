// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Counterparties;

/// <summary>Values for filtering counterparties.</summary>
/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
public sealed partial class CounterpartyFilter(IActivityService activityService) : FilterBase<CounterpartyRow>(activityService)
{
	/// <summary>Gets or sets the text by which to filter counterparty.</summary>
	[Notify]
	private string? _filterText;

	/// <inheritdoc />
	protected override bool FilterRow(CounterpartyRow row) =>
		FilterText is null ||
		row.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
}
