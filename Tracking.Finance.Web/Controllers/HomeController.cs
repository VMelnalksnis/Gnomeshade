using System.Diagnostics;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Tracking.Finance.Web.Models;

namespace Tracking.Finance.Web.Controllers
{
	/// <summary>
	/// Contains views for home page accessible by all users.
	/// </summary>
	[AllowAnonymous]
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="HomeController"/> class.
		/// </summary>
		/// <param name="logger">Logger used in the context of this controller.</param>
		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Returns the home page view.
		/// </summary>
		/// <returns>Home page view.</returns>
		[HttpGet]
		public ViewResult Index() => View();

		/// <summary>
		/// Returns the privacy information page view.
		/// </summary>
		/// <returns>Privacy information page view.</returns>
		[HttpGet]
		public ViewResult Privacy() => View();

		/// <summary>
		/// Returns a view containg information about an internal error.
		/// </summary>
		/// <returns>Interal error information page view.</returns>
		[HttpGet]
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public ViewResult Error()
		{
			var viewModel =
				new ErrorViewModel
				{
					RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
				};

			_logger.LogError("Error occured for request {RequestId}", viewModel.RequestId);
			return View(viewModel);
		}
	}
}
