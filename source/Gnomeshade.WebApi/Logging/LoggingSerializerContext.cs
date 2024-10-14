// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Text.Json.Serialization;

using VMelnalksnis.NordigenDotNet.Accounts;

namespace Gnomeshade.WebApi.Logging;

/// <inheritdoc cref="JsonSerializerContext" />
[JsonSourceGenerationOptions(IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(BookedTransaction))]
internal sealed partial class LoggingSerializerContext : JsonSerializerContext;
