// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Gnomeshade.Interfaces.Desktop.Configuration;

internal sealed record GnomeshadeOptions([property: Required] Uri BaseAddress)
{
	internal const string _sectionName = "Gnomeshade";
}
