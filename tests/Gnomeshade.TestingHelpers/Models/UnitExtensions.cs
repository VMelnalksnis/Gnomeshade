﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Gnomeshade.WebApi.Models.Products;

namespace Gnomeshade.TestingHelpers.Models;

public static class UnitExtensions
{
	public static UnitCreation ToCreation(this Unit unit) => new()
	{
		OwnerId = unit.OwnerId,
		Name = unit.Name,
		ParentUnitId = unit.ParentUnitId,
		Multiplier = unit.Multiplier,
	};
}
