﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.Avalonia.Core.Configuration;

/// <summary>Settings for accessing gnomeshade API.</summary>
[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.Members)]
public sealed record GnomeshadeOptions
{
	/// <summary>Gets or sets the gnomeshade API base address.</summary>
	[Required]
	public Uri BaseAddress { get; set; } = null!;
}
