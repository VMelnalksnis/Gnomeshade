using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using NUnit.Framework;

using VMelnalksnis.SvgCharts.Charts;

namespace VMelnalksnis.SvgCharts.Tests
{
	public class LineChartExtensionsTestCaseSource : IEnumerable
	{
		public IEnumerator GetEnumerator()
		{
			yield return
				new TestCaseData(
					new LineChart
					{
						HorizontalAxisName = "X",
						VerticalAxisName = "Y",
						ViewBox = new Rectangle(0, 0, 500, 100),
						Points = new List<Point>(),
					},
					"<svg viewBox=\"0 0 500 100\" class=\"chart\">\r\n</svg>")
				.SetName("Line chart without points");

			yield return
				new TestCaseData(
					new LineChart
					{
						HorizontalAxisName = "X",
						VerticalAxisName = "Y",
						ViewBox = new Rectangle(0, 0, 500, 100),
						Points = new List<Point>
						{
							new Point(0, 0),
							new Point(10, 10),
							new Point(20, 10),
						},
					},
					"<svg viewBox=\"0 0 500 100\" class=\"chart\">\r\n<polyline fill=\"none\" stroke=\"#0074d9\" stroke-width=\"2\" points=\"0,0 10,10 20,10\"/>\r\n</svg>")
				.SetName("Line chart with multiple points");
		}
	}
}
