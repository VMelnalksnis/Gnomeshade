using System.Collections;
using System.Collections.Generic;

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
						ViewBox = new Rectangle(0, 0, 500, 100),
						Datasets = new List<Dataset>(),
						HorizontalAxis = new Axis(0, 100, "X", null, null),
						VerticalAxis = new Axis(0, 500, "Y", null, null),
					},
					"<svg viewBox=\"0 0 500 100\" class=\"chart\">\r\n</svg>")
				.SetName("Line chart without points");

			yield return
				new TestCaseData(
					new LineChart
					{
						ViewBox = new Rectangle(0, 0, 500, 100),
						Datasets = new List<Dataset>
						{
							new Dataset(new List<Point> { new Point(0, 0), new Point(10, 10), new Point(20, 10), }),
						},
						HorizontalAxis = new Axis(0, 100, "X", null, null),
						VerticalAxis = new Axis(0, 500, "Y", null, null),
					},
#pragma warning disable SA1118 // Parameter should not span multiple lines
					"<svg viewBox=\"0 0 500 100\" class=\"chart\">\r\n<polyline fill=\"none\" stroke=\"#0074d9\" stroke-width=\"2\" points=\"0,0 10,10 20,10\"/>\r\n" +
					"<g class=\"data\" data-setname=\"test data set\">\r\n" +
					"<circle cx=\"0\" cy=\"0\" data-value=\"0\" r=\"2\"></circle>\r\n" +
					"<circle cx=\"10\" cy=\"10\" data-value=\"10\" r=\"2\"></circle>\r\n" +
					"<circle cx=\"20\" cy=\"10\" data-value=\"10\" r=\"2\"></circle>\r\n" +
					"</g>\r\n" +
					"</svg>")
#pragma warning restore SA1118 // Parameter should not span multiple lines
				.SetName("Line chart with multiple points");
		}
	}
}
