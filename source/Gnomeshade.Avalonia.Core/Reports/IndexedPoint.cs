// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Text.Json.Serialization;

using LiveChartsCore.Kernel;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Reports;

/// <summary>Defines a point which uses its index in the values collection as the X coordinate.</summary>
public sealed partial class IndexedPoint : PropertyChangedBase, IChartEntity
{
	/// <summary>Gets or sets the value of the point.</summary>
	[Notify]
	private double _value;

	/// <summary>Initializes a new instance of the <see cref="IndexedPoint"/> class.</summary>
	/// <param name="value">The value of the point.</param>
	public IndexedPoint(double value)
	{
		MetaData = new(OnCoordinateChanged);
		Value = value;
	}

	/// <inheritdoc cref="IChartEntity.MetaData"/>
	[JsonIgnore]
	public ChartEntityMetaData? MetaData { get; set; }

	/// <inheritdoc cref="IChartEntity.Coordinate"/>
	[JsonIgnore]
	public Coordinate Coordinate { get; set; } = Coordinate.Empty;

	private void OnCoordinateChanged(int index) => Coordinate = new(index, Value);
}
