using System.Threading.Tasks;

using Tracking.Finance.Data.Models.Abstractions;

namespace Tracking.Finance.Data.Repositories
{
	public interface IModifiableRepository<TEntity>
		where TEntity : class, IEntity, IModifiableEntity
	{
		Task<int> UpdateAsync(TEntity entity);
	}
}
