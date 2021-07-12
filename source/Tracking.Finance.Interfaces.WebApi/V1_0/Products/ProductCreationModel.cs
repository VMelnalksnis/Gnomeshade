// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

namespace Tracking.Finance.Interfaces.WebApi.V1_0.Products
{
	public sealed record ProductCreationModel
	{
		public string? Name { get; init; }

		public string? Description { get; init; }
	}
}
