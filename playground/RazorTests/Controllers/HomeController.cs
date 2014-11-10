using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using WebMatrix.WebData;

using RazorTests.Models;

namespace RazorTests.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult About()
		{
			return View();
		}

		public ActionResult Contact()
		{
			return View();
		}

		// [ValidateAntiForgeryToken]
		//		public ActionResult Index(LoginModel model, string returnUrl)

		[HttpPost]
		[AllowAnonymous]
		public ActionResult Index(string email, string password, bool? rememberMe)
		{
			if (ModelState.IsValid && WebSecurity.Login(email, password, persistCookie: rememberMe ?? false))
			{
				return View();
			}

			// If we got this far, something failed, redisplay form
			ModelState.AddModelError("", "The user name or password provided is incorrect.");

			return View();
			// return View(model);
		}

		private ActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			else
			{
				return RedirectToAction("Index", "Home");
			}
		}
	}
}
