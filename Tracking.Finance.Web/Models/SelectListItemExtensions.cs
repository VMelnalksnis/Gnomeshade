using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models
{
	public static class SelectListItemExtensions
	{
		public static SelectListItem GetSelectListItem<TEntity>(this TEntity entity)
			where TEntity : IEntity, INamedEntity
		{
			return new SelectListItem(entity.Name, entity.Id.ToString());
		}

		public static List<SelectListItem> GetSelectListItems<TEntity>(this IEnumerable<TEntity> entities)
			where TEntity : IEntity, INamedEntity
		{
			var listItems = new List<SelectListItem>();
			listItems.AddRange(entities.Select(entity => entity.GetSelectListItem()));
			return listItems;
		}

		public static List<SelectListItem> GetSelectListItemsWithDefault<TEntity>(
			this IEnumerable<TEntity> entities,
			string? name = null,
			string? value = null,
			bool disabled = false)
			where TEntity : IEntity, INamedEntity
		{
			var listItems = new List<SelectListItem>()
			{
				new SelectListItem(name, value, true, disabled),
			};

			listItems.AddRange(entities.Select(entity => entity.GetSelectListItem()));
			return listItems;
		}

		public static IQueryable<TEntity> WhichBelongToUser<TEntity>(this IQueryable<TEntity> entitySet, FinanceUser financeUser)
			where TEntity : class, IUserSpecificEntity
		{
			return entitySet.Where(entity => entity.FinanceUserId == financeUser.Id);
		}

		public static IQueryable<TEntity> WithId<TEntity>(this IQueryable<TEntity> entitySet, int id)
			where TEntity : class, IEntity
		{
			return entitySet.Where(entity => entity.Id == id);
		}
	}
}
