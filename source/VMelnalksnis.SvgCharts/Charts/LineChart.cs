using System.Collections.Generic;

namespace VMelnalksnis.SvgCharts.Charts
{
	public class LineChart
	{
		public Rectangle ViewBox { get; set; }

		public List<Dataset> Datasets { get; set; }

		public Axis HorizontalAxis { get; set; }

		public Axis VerticalAxis { get; set; }
	}
}
