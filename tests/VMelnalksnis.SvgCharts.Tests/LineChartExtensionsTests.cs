using System.IO;

using FluentAssertions;

using Microsoft.Extensions.WebEncoders.Testing;

using NUnit.Framework;

using VMelnalksnis.SvgCharts.Charts;

namespace VMelnalksnis.SvgCharts.Tests
{
	public class LineChartExtensionsTests
	{
		[TestCaseSource(typeof(LineChartExtensionsTestCaseSource))]
		public void Display_ShouldReturnExpected(LineChart lineChart, string expectedHtml)
		{
			var htmlContent = lineChart.Display();

			var stringWriter = new StringWriter();
			htmlContent.WriteTo(stringWriter, new HtmlTestEncoder());
			var html = stringWriter.ToString();

			html.Should().Be(expectedHtml);
		}
	}
}
