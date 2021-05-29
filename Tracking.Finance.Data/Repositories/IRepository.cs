using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Repositories
{
	public interface IRepository<TEntity>
		where TEntity : class, IEntity
	{
		Task<int> AddAsync(TEntity entity);

		Task<TEntity> GetByIdAsync(int id, CancellationToken cancellationToken = default);

		Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

		Task<int> DeleteAsync(int id);
	}
}
