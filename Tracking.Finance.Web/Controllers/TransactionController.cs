﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Tracking.Finance.Web.Data;
using Tracking.Finance.Web.Data.Models;
using Tracking.Finance.Web.Models;
using Tracking.Finance.Web.Models.Transactions;

namespace Tracking.Finance.Web.Controllers
{
	[Authorize]
	public class TransactionController : FinanceController
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionController"/> class.
		/// </summary>
		public TransactionController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
			: base(dbContext, userManager)
		{
		}

		[HttpGet]
		public async Task<ViewResult> Index(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);

			var transactions =
				await DbContext.Transactions
					.WhichBelongToUser(financeUser)
					.Include(transaction => transaction.TransactionItems)
					.Take(10)
					.ToListAsync(cancellationToken);

			var viewModel = new TransactionIndexViewModel(transactions);
			return View(viewModel);
		}

		[HttpGet]
		public async Task<ViewResult> Details(int id, CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);

			var transaction =
				await DbContext.Transactions
					.WhichBelongToUser(financeUser)
					.SingleAsync(transaction => transaction.Id == id, cancellationToken);

			var items =
				await DbContext.TransactionItems
					.Where(item => item.TransactionId == transaction.Id)
					.Include(item => item.SourceCurrency)
					.Include(item => item.Product)
					.ToListAsync(cancellationToken);

			var viewModelItems = items.Select(item => new TransactionDetailsItemModel(item)).ToList();
			var viewModel = new TransactionDetailsModel(transaction, viewModelItems);
			return View(viewModel);
		}

		[HttpGet]
		public async Task<ViewResult> Create(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);

			var accounts =
				await DbContext.Accounts
					.WhichBelongToUser(financeUser)
					.ToListAsync(cancellationToken);

			var categories =
				await DbContext.TransactionCategories
					.WhichBelongToUser(financeUser)
					.ToListAsync(cancellationToken);

			var counterparties =
				await DbContext.Counterparties
					.WhichBelongToUser(financeUser)
					.ToListAsync(cancellationToken);

			var accountItems = accounts.GetSelectListItems();
			var categoryItems = categories.GetSelectListItems();
			var counterpartyItems = counterparties.GetSelectListItems();

			var viewModel = new TransactionCreationModel
			{
				Accounts = accountItems,
				Categories = categoryItems,
				Counterparties = counterpartyItems,
				FinanceUserId = financeUser.Id,
			};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Create(TransactionCreationModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var transaction = model.Map().SetCreationDate();
			var entity = await DbContext.Transactions.AddAsync(transaction);
			await SaveChangesAsync();

			return RedirectToAction(nameof(Details), new { id = entity.Entity.Id });
		}

		[HttpGet]
		public async Task<ViewResult> CreateItem(int id, CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);

			var transaction =
				await DbContext.Transactions
					.WhichBelongToUser(financeUser)
					.SingleAsync(transaction => transaction.Id == id, cancellationToken);

			var currencies =
				await DbContext.Currencies
					.ToListAsync(cancellationToken);

			var products =
				await DbContext.Products
					.WhichBelongToUser(financeUser)
					.ToListAsync(cancellationToken);

			var currencyItems = currencies.GetSelectListItems();
			var productItems = products.GetSelectListItems();

			var viewModel = new TransactionItemCreationModel
			{
				FinanceUserId = transaction.FinanceUserId,
				TransactionId = transaction.Id,
				Currencies = currencyItems,
				Products = productItems,
			};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> CreateItem(TransactionItemCreationModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var transactionItem = model.Map().SetCreationDate();
			var entity = await DbContext.TransactionItems.AddAsync(transactionItem);
			await SaveChangesAsync();

			return RedirectToAction(nameof(Details), new { id = entity.Entity.TransactionId });
		}

		[HttpGet]
		public async Task<ViewResult> CreateCategory(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);

			var viewModel = new TransactionCategoryCreationModel
			{
				FinanceUserId = financeUser.Id,
			};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> CreateCategory(TransactionCategoryCreationModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var transactionCategory = new TransactionCategory
			{
				FinanceUserId = model.FinanceUserId.Value,
				Name = model.Name,
				NormalizedName = model.Name.ToUpperInvariant().Trim(),
			};

			var entity = await DbContext.TransactionCategories.AddAsync(transactionCategory);
			await SaveChangesAsync();

			return RedirectToAction(nameof(Index)); // todo
		}
	}
}
