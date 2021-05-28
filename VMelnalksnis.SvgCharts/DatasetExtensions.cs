using System.Linq;

using Microsoft.AspNetCore.Html;

using VMelnalksnis.SvgCharts.Charts;

namespace VMelnalksnis.SvgCharts
{
	public static class DatasetExtensions
	{
		public static void Display(this Dataset dataset, IHtmlContentBuilder builder)
		{
			if (dataset.Points.Any())
			{
				builder.AppendHtml($"<polyline fill=\"none\" stroke=\"#0074d9\" stroke-width=\"2\" points=\"");
				var linePoints =
					dataset.Points
						.Select(point => $"{point.X},{point.Y}")
						.Aggregate((pointA, pointB) => $"{pointA} {pointB}");
				builder.AppendHtml(linePoints);
				builder.AppendHtmlLine("\"/>");

				builder.AppendHtmlLine($"<g class=\"data\" data-setname=\"test data set\">");
				foreach (var point in dataset.Points)
				{
					builder.AppendHtmlLine($"<circle cx=\"{point.X}\" cy=\"{point.Y}\" data-value=\"{point.Y}\" r=\"2\"></circle>");
				}

				builder.AppendHtmlLine($"</g>");
			}
		}
	}
}
