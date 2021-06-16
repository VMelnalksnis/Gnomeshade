using System;
using System.Linq.Expressions;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

using VMelnalksnis.SvgCharts.Charts;

namespace VMelnalksnis.SvgCharts
{
	public static class HtmlHelperExtensions
	{
		public static IHtmlContent Display<TModel, TChart>(
			this IHtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TChart>> expression)
			where TChart : LineChart
		{
			var resultFunction = expression.Compile();
			var chart = resultFunction(htmlHelper.ViewData.Model);
			return chart.Display();
		}
	}
}
