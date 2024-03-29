﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.DesignTime;
using Gnomeshade.Avalonia.Core.Transactions.Links;

namespace Gnomeshade.Avalonia.Core.Tests.Transactions.Links;

[TestOf(typeof(LinkUpsertionViewModel))]
public sealed class LinkUpsertionViewModelTests
{
	[Test]
	public async Task SaveAsync_ShouldLinkAnExistingLink()
	{
		const string linkValue = "https://example.com/";
		var client = new DesignTimeGnomeshadeClient();

		for (var i = 0; i < 2; i++)
		{
			var viewModel = new LinkUpsertionViewModel(new StubbedActivityService(), client, Guid.Empty, null);
			await viewModel.RefreshAsync();

			viewModel.UriValue = linkValue;
			await viewModel.SaveAsync();

			(await client.GetLinksAsync()).Should().ContainSingle(link => link.Uri == linkValue);
		}
	}
}
