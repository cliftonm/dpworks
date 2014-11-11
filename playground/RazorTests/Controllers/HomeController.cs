using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Helpers;		// For WebMail

using WebMatrix.WebData;

using RazorEngine;

using RazorTests.Models;

namespace RazorTests.Controllers
{
	public class HomeController : Controller
	{
		[AllowAnonymous]
		public ActionResult Index()
		{
			if (User.Identity.IsAuthenticated)
			{
				if (HttpContext.Session["SiteName"] == null)
				{
					HttpContext.Session["SiteName"] = "";
				}
			}

			if (User.IsInRole("Site-Wide Administrator"))
			{
				var context = new UsersContext();
				var sites = context.SiteProfiles;
				ViewBag.Sites = sites;
			}

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

		//[HttpPost]
		//[ValidateAntiForgeryToken]
		[Authorize]
		public ActionResult Logout()
		{
			WebSecurity.Logout();

			return RedirectToAction("Index", "Home");
		}

		// [ValidateAntiForgeryToken]

		/// <summary>
		/// This is the login postback.
		/// </summary>
		[HttpPost]
		[AllowAnonymous]
		public ActionResult Index(string email, string password, bool? rememberMe)
		{
			if (ModelState.IsValid && WebSecurity.Login(email, password, persistCookie: rememberMe ?? false))
			{
				return RedirectToAction("Index", "Home");
			}

			return View();
			// return View(model);
		}

		[AllowAnonymous]
		public ActionResult RegisterNewUser(string token)
		{
			var context = new UsersContext();
			UserInfo ui = context.UserInfo.SingleOrDefault(u => u.RegistrationToken == token);

			if (ui != null)
			{
				ViewBag.UserInfo = ui;
				ViewBag.Token = token;

				return View();
			}

			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		[AllowAnonymous]
		public ActionResult RegisterNewUser(string password, string confirmPassword, string token)
		{
			var context = new UsersContext();
			UserInfo ui = context.UserInfo.SingleOrDefault(u => u.RegistrationToken == token);

			if (ui != null)
			{
				// Clear out the registration token so it can't be used again.
				ui.RegistrationToken = "";
				ui.Activated = true;
				context.SaveChanges();

				// Create the account and log in.
				WebSecurity.CreateUserAndAccount(ui.Email, password);
				WebSecurity.Login(ui.Email, password);
			}

			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		[Authorize]
		public ActionResult SelectSite(int siteId)
		{
			var context = new UsersContext();
			var site = context.SiteProfiles.Find(siteId);
			HttpContext.Session["SiteName"] = site.Name;
			HttpContext.Session["SiteId"] = site.Id;
			return Json(new { Name = site.Name });
		}

		/*
		[AllowAnonymous]
		public ActionResult EmailTest()
		{
			WebMail.SmtpServer = System.Configuration.ConfigurationManager.AppSettings["SmtpServer"];
			WebMail.UserName = System.Configuration.ConfigurationManager.AppSettings["Username"];
			WebMail.Password = System.Configuration.ConfigurationManager.AppSettings["Password"];
			WebMail.From = System.Configuration.ConfigurationManager.AppSettings["Username"];
			WebMail.SmtpPort = 587;
			WebMail.EnableSsl = false;

			// http://stackoverflow.com/questions/4368815/razor-views-as-email-templates

			VirtualPathProvider vpp = HostingEnvironment.VirtualPathProvider;
			Stream s = vpp.GetFile("~/Templates/NewUserRegistrationEmail.cshtml").Open();
			StreamReader sr = new StreamReader(s);
			string template = sr.ReadToEnd();
			sr.Close();
			s.Close();

			string html = Razor.Parse(template, new { FirstName = "Marc", Token = "abc" });

			WebMail.Send(to: "marc.clifton@gmail.com", subject: "Test2", body: html);

			return RedirectToAction("Index", "Home");
		}
		 */
	}
}
