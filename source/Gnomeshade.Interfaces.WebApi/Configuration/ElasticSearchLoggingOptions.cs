﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

using static JetBrains.Annotations.ImplicitUseKindFlags;
using static JetBrains.Annotations.ImplicitUseTargetFlags;

namespace Gnomeshade.Interfaces.WebApi.Configuration;

/// <summary>Options for configuring logging to ElasticSearch.</summary>
[UsedImplicitly(Assign, Members)]
public sealed class ElasticSearchLoggingOptions
{
	/// <summary>Gets the ElasticSearch nodes to which to log to.</summary>
	[Required]
	[MinLength(1)]
	public List<Uri> Nodes { get; init; } = null!;

	/// <summary>Gets the username to use for basic authentication.</summary>
	[Required]
	public string Username { get; init; } = null!;

	/// <summary>Gets the password to use for basic authentication.</summary>
	[Required]
	public string Password { get; init; } = null!;
}
