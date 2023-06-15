// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using HtmlAgilityPack;

namespace Gnomeshade.WebApi.Tests.Integration.Oidc;

internal static class HtmlDocumentExtensions
{
	internal static FormData GetForm(this HtmlDocument htmlDocument, string xpath)
	{
		var formNode = htmlDocument.DocumentNode.SelectSingleNode(xpath);
		var action = formNode.Attributes["action"].Value.Replace("&amp;", "&");

		var buttonNodes = formNode
				.SelectNodes($"{xpath}//button")
				?.Where(node => node.Attributes.Contains("name")) ??
			Array.Empty<HtmlNode>();

		var inputNodes = formNode.SelectNodes($"{xpath}//input")?.ToArray() ?? Array.Empty<HtmlNode>();

		var inputs = inputNodes
			.Concat(buttonNodes)
			.Select(node =>
			{
				var name = node.Attributes["name"]?.Value ??
					throw new InvalidOperationException("Input element does not have a name");
				var value = node.Attributes["value"]?.Value;

				return new KeyValuePair<string, string?>(name, value);
			})
			.GroupBy(pair => pair.Key)
			.Select(grouping =>
			{
				var value = grouping.Select(pair => pair.Value).Distinct().Single();
				return new KeyValuePair<string, string?>(grouping.Key, value);
			});

		return new(action, new(inputs));
	}
}

internal sealed record FormData(string Action, FormUrlEncodedContent Content);
