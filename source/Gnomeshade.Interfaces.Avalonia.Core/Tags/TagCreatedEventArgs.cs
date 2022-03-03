﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tags;

/// <summary>Event arguments for <see cref="TagCreationViewModel.TagCreated"/> event.</summary>
public sealed class TagCreatedEventArgs : EventArgs
{
	/// <summary>Initializes a new instance of the <see cref="TagCreatedEventArgs"/> class.</summary>
	/// <param name="tagId">The id of the created tag.</param>
	public TagCreatedEventArgs(Guid tagId)
	{
		TagId = tagId;
	}

	/// <summary>Gets the id of the created tag.</summary>
	public Guid TagId { get; }
}
