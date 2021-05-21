using System.Collections.Generic;
using System.Drawing;

namespace VMelnalksnis.SvgCharts.Charts
{
	public class LineChart
	{
		public string? HorizontalAxisName { get; set; }

		public string? VerticalAxisName { get; set; }

		public Rectangle ViewBox { get; set; }

		public List<Point> Points { get; set; }
	}
}
