﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

namespace Gnomeshade.WebApi.Models;

/// <summary>Information needed to create a link.</summary>
/// <seealso cref="Link"/>
[PublicAPI]
public sealed record LinkCreation : Creation
{
	/// <inheritdoc cref="Link.Uri"/>
	public Uri? Uri { get; set; }
}
