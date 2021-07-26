// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.WebApi.V1_0.Products
{
	[PublicAPI]
	public sealed record ProductCreationModel
	{
		[Required(AllowEmptyStrings = false)]
		public string? Name { get; init; }

		public string? Description { get; init; }

		public Guid? UnitId { get; init; }
	}
}
