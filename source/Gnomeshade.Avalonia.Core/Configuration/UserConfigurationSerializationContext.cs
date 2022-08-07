// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Gnomeshade.Avalonia.Core.Configuration;

/// <inheritdoc />
[JsonSerializable(typeof(UserConfiguration))]
internal partial class UserConfigurationSerializationContext : JsonSerializerContext
{
}
