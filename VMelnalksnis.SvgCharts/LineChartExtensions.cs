using System.Linq;

using Microsoft.AspNetCore.Html;

using VMelnalksnis.SvgCharts.Charts;

namespace VMelnalksnis.SvgCharts
{
	public static class LineChartExtensions
	{
		public static IHtmlContent Display(this LineChart lineChart)
		{
			var builder = new HtmlContentBuilder();
			var viewBox = $"{lineChart.ViewBox.X} {lineChart.ViewBox.Y} {lineChart.ViewBox.Width} {lineChart.ViewBox.Height}";
			builder.AppendHtmlLine($"<svg viewBox=\"{viewBox}\" class=\"chart\">");

			foreach (var dataset in lineChart.Datasets)
			{
				dataset.Display(builder);
			}

			builder.AppendHtml(@"</svg>");

			return builder;
		}
	}
}
