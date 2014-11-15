using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using WebMatrix.WebData;
using RazorTests.Filters;
using RazorTests.Models;

namespace RazorTests.Controllers
{
	[Authorize]
	[InitializeSimpleMembership]
	public class AccountController : Controller
	{
		// Change this to Authorize for the production version.
		// [AllowAnonymous]
		[Authorize]
		public ActionResult Register()
		{
			return View();
		}

		// !!! THIS METHOD IS ENABLED ONLY WHEN WE NEED TO SEED THE DB WITH A SITE-ADMIN USER !!!
		// [Authorize]
		[AllowAnonymous]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Register(RegisterModel model)
		{
			// Attempt to register the user
			WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
			WebSecurity.Login(model.UserName, model.Password);
			return RedirectToAction("Index", "Home");
		}
	}
}
