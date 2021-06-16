using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Tracking.Finance.Web.Data;
using Tracking.Finance.Web.Data.Models;
using Tracking.Finance.Web.Models;
using Tracking.Finance.Web.Models.Products;

namespace Tracking.Finance.Web.Controllers
{
	[Authorize]
	public class ProductController : FinanceController
	{
		public ProductController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
			: base(dbContext, userManager)
		{
		}

		[HttpGet]
		public async Task<ViewResult> Index(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);

			var products =
				await DbContext.Products
					.WhichBelongToUser(financeUser)
					.Include(product => product.ProductCategory)
					.Include(product => product.Supplier)
					.Include(product => product.Unit)
					.ToListAsync(cancellationToken);

			var viewModel =
				products
					.Select(product => new ProductIndexModel(
						product.Id,
						product.ProductCategoryId,
						product.ProductCategory.Name,
						product.Unit.Name,
						product.SupplierId,
						product.Supplier.Name,
						product.Name,
						product.Description))
					.ToList();

			return View(viewModel);
		}

		[HttpGet]
		public async Task<ViewResult> Details(int id, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		[HttpGet]
		public async Task<ViewResult> Create(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);

			var categories =
				await DbContext.ProductCategories
					.WhichBelongToUser(financeUser)
					.ToListAsync(cancellationToken);

			var suppliers =
				await DbContext.Counterparties
					.WhichBelongToUser(financeUser)
					.ToListAsync(cancellationToken);

			var units = await DbContext.Units.ToListAsync(cancellationToken);

			var viewModel = new ProductCreationModel
			{
				FinanceUserId = financeUser.Id,
				Categories = categories.GetSelectListItems(),
				Suppliers = suppliers.GetSelectListItems(),
				Units = units.GetSelectListItems(),
			};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Create(ProductCreationModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var product = new Product
			{
				FinanceUserId = model.FinanceUserId.Value,
				ProductCategoryId = model.ProductCategoryId.Value,
				SupplierId = model.SupplierId.Value,
				UnitId = model.UnitId.Value,
				Description = model.Description,
			}.WithName(model.Name).CreatedAndModifiedNow();

			var entity = await DbContext.Products.AddAsync(product);
			await SaveChangesAsync();

			return RedirectToAction(nameof(Details), new { id = entity.Entity.Id });
		}

		[HttpGet]
		public async Task<ViewResult> CreateCategory(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);
			var categories =
				await DbContext.ProductCategories
					.WhichBelongToUser(financeUser)
					.ToListAsync(cancellationToken);

			var viewModel = new ProductCategoryCreationModel
			{
				FinanceUserId = financeUser.Id,
				Categories = categories.GetSelectListItemsWithDefault(),
			};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> CreateCategory(ProductCategoryCreationModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var productCategory =
				new ProductCategory { FinanceUserId = model.FinanceUserId.Value, }
					.WithName(model.Name);

			var entity = await DbContext.ProductCategories.AddAsync(productCategory);
			await SaveChangesAsync();

			if (model.ParentCategoryId.HasValue)
			{
				var closure = new ProductCategoryClosure
				{// todo depth
					ParentProductCategoryId = model.ParentCategoryId.Value,
					ChildProductCategoryId = entity.Entity.Id,
				};
				var closureEntity = await DbContext.ProductCategoryClosures.AddAsync(closure);
				await SaveChangesAsync();
			}

			return RedirectToAction(nameof(Index)); // todo details
		}
	}
}
