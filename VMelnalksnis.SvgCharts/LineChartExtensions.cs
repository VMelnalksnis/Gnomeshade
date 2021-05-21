using System.Linq;

using Microsoft.AspNetCore.Html;

using VMelnalksnis.SvgCharts.Charts;

namespace VMelnalksnis.SvgCharts
{
	public static class LineChartExtensions
	{
		public static IHtmlContent Display(this LineChart lineChart)
		{
			var htmlContentBuilder = new HtmlContentBuilder();
			var viewBox = $"{lineChart.ViewBox.X} {lineChart.ViewBox.Y} {lineChart.ViewBox.Width} {lineChart.ViewBox.Height}";
			htmlContentBuilder.AppendHtmlLine($"<svg viewBox=\"{viewBox}\" class=\"chart\">");

			if (lineChart.Points.Any())
			{
				htmlContentBuilder.AppendHtml($"<polyline fill=\"none\" stroke=\"#0074d9\" stroke-width=\"2\" points=\"");
				var points =
					lineChart.Points
						.Select(point => $"{point.X},{point.Y}")
						.Aggregate((pointA, pointB) => $"{pointA} {pointB}");
				htmlContentBuilder.AppendHtml(points);
				htmlContentBuilder.AppendHtmlLine("\"/>");
			}

			htmlContentBuilder.AppendHtml(@"</svg>");

			return htmlContentBuilder;
		}
	}
}
