// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using JetBrains.Annotations;

using Microsoft.OpenApi.Models;

using NodaTime;

namespace Gnomeshade.WebApi.OpenApi;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class InstantSchemaFilter : SchemaFilter<Instant>
{
	/// <inheritdoc/>
	protected override void ApplyFilter(OpenApiSchema schema)
	{
		schema.Type = "string";
		schema.Format = "date-time";
	}
}
