using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Tracking.Finance.Web.Data;
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
					.ToListAsync(cancellationToken);

			var viewModel =
				transactions
					.Select(transaction =>
						{
							var items =
								DbContext.TransactionItems
									.WhichBelongToUser(financeUser)
									.Where(item => item.TransactionId == transaction.Id)
									.Include(item => item.SourceAccount)
									.ThenInclude(account => account.Currency)
									.Include(item => item.TargetAccount)
									.ThenInclude(account => account.Currency)
									.ToList();

							var x =
								items
									.GroupBy(item => item.SourceAccount.Currency.AlphabeticCode)
									.Select(grouping => new CurrencyAmount { Currency = grouping.Key, SourceAmount = grouping.Sum(item => item.SourceAmount) });

							var firstItem = items.FirstOrDefault();
							var firstItemSourceId = firstItem?.SourceAccountId;
							var firstItemTargetId = firstItem?.TargetAccountId;

							var sourceAccount =
								DbContext.Accounts
									.WhichBelongToUser(financeUser)
									.Where(account => account.Id == firstItemSourceId)
									.SingleOrDefault();

							var targetAccount =
								DbContext.Accounts
									.WhichBelongToUser(financeUser)
									.Where(account => account.Id == firstItemTargetId)
									.SingleOrDefault();

							return
								new TransactionIndexViewModel(
									transaction.Id,
									sourceAccount?.Id,
									sourceAccount?.Name,
									targetAccount?.Id,
									targetAccount?.Name,
									transaction.Date.LocalDateTime,
									items.Sum(item => item.SourceAmount),
									firstItem?.SourceAccount.Currency.AlphabeticCode,
									items.Sum(item => item.TargetAmount),
									firstItem?.TargetAccount.Currency.AlphabeticCode);
						})
					.ToList();

			return View(viewModel);
		}

		[HttpGet]
		public async Task<ViewResult> Details(int id, CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);

			var transaction =
				await DbContext.Transactions
					.WhichBelongToUser(financeUser)
					.WithId(id)
					.SingleAsync(cancellationToken);

			var items =
				await DbContext.TransactionItems
					.Where(item => item.TransactionId == transaction.Id)
					.Include(item => item.SourceAccount)
					.ThenInclude(account => account.Currency)
					.Include(item => item.SourceAccount)
					.ThenInclude(account => account.Account)
					.Include(item => item.TargetAccount)
					.ThenInclude(account => account.Currency)
					.Include(item => item.TargetAccount)
					.ThenInclude(account => account.Account)
					.Include(item => item.Product)
					.ToListAsync(cancellationToken);

			var viewModelItems = items
				.Select(item => new TransactionDetailsItemModel(item)
				{
					SourceAccount = item.SourceAccount.Account.Name,
					SourceAccountId = item.SourceAccount.Account.Id,
					TargetAccount = item.TargetAccount.Account.Name,
					TargetAccountId = item.TargetAccount.Account.Id,
					ProductId = item.Product.Id,
					Date = transaction.Date.LocalDateTime,
				})
				.ToList();
			var viewModel = new TransactionDetailsModel(transaction, viewModelItems);
			return View(viewModel);
		}

		[HttpGet]
		public async Task<ViewResult> Create(CancellationToken cancellationToken)
		{
			var financeUser = await GetCurrentUser(cancellationToken);

			var viewModel = new TransactionCreationModel
			{
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

			var transaction = model.Map().CreatedAndModifiedNow();
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
					.WithId(id)
					.SingleAsync(cancellationToken);

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

			var transactionItem = model.Map().CreatedAndModifiedNow();
			var entity = await DbContext.TransactionItems.AddAsync(transactionItem);
			await SaveChangesAsync();

			return RedirectToAction(nameof(Details), new { id = entity.Entity.TransactionId });
		}
	}
}
