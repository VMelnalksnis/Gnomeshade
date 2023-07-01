// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Gnomeshade.Avalonia.Core.Help;

/// <inheritdoc cref="System.Text.Json.Serialization.JsonSerializerContext" />
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(List<PackageInfo>))]
internal sealed partial class PackageContext : JsonSerializerContext
{
}
