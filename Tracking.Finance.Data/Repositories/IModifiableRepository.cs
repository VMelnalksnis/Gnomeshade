﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU General Public License 3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Repositories
{
	public interface IModifiableRepository<TEntity>
		where TEntity : class, IEntity, IModifiableEntity
	{
		Task UpdateAsync(TEntity entity);
	}
}
