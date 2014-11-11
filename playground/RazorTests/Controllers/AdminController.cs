using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;			// need for VirtualPathProvider.
using System.Web.Mvc;
using System.Web.Mvc.Html;			// needed for HtmlHelper class extensions.
using WebMatrix.Data;
using WebMatrix.WebData;

using RazorEngine;					// used for razor engine to generate html from cshtml templates.

using RazorTests.Models;

namespace RazorTests.Controllers
{
    public class AdminController : Controller
    {
		// Change this to Authorize for the production version.
		[AllowAnonymous]
		public ActionResult RoleManagement()
        {
            return View();
        }

		// ========= SITE MANAGEMENT ==============

		[Authorize]
		public ActionResult SiteManagement()
		{
			// var db = Database.Open("DefaultConnection");
			var context = new UsersContext();
			var sites = context.SiteProfiles;

			HtmlHelper helper = this.GetHtmlHelper();

			// Initialize grid metadata
			GridMetadata gridMetadata = new GridMetadata();
			gridMetadata.AddColumn("col2", "name", ColumnMetadata.Control.TextBox, "Name", "Name", "20");
			gridMetadata.AddColumn("col3", "municipality", ColumnMetadata.Control.TextBox, "Municipality", "Municipality", "15");
			gridMetadata.AddColumn("col4", "state", ColumnMetadata.Control.TextBox, "State", "State", "5");
			gridMetadata.AddColumn("col5", "contactname", ColumnMetadata.Control.TextBox, "ContactName", "Contact Name", "25");
			gridMetadata.AddColumn("col6", "contactphone", ColumnMetadata.Control.TextBox, "ContactPhone", "Contact Phone", "25");
			gridMetadata.AddColumn("col7", "contactemail", ColumnMetadata.Control.TextBox, "ContactEmail", "Contact Email", "25");

			// Initialize grid
			WebGrid grid = new WebGrid(sites, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "setArrows");

			List<WebGridColumn> columnSet = gridMetadata.GetColumnSet(helper, grid);

			// Look at EngineContext.Current.Resolve<GenericController>() to be able to call the controller methods directly.
			// See http://stackoverflow.com/questions/5960664/calling-a-method-in-the-controller

			ViewBag.Grid = grid;
			ViewBag.ColumnSet = columnSet;
			ViewBag.InlineNewRow = gridMetadata.GetInlineNewRow();
			ViewBag.EditGetters = gridMetadata.EditGetters();
			ViewBag.DisplaySetters = gridMetadata.DisplaySetters();
			ViewBag.PostPath = "/Admin/SaveSite";
			ViewBag.DeletePath = "/Admin/DeleteSite";
			ViewBag.PostbackParams = gridMetadata.PostbackParams();
			ViewBag.PopulateDropDownLists = gridMetadata.PopulateDropDownLists();

			return View();
		}

		[HttpPost]
		[Authorize]
		public ActionResult DeleteSite()
		{
			int id = Convert.ToInt32(Request["ItemId"]);
			var context = new UsersContext();
			SiteProfile sp = context.SiteProfiles.Find(id);

			context.SiteProfiles.Remove(sp);
			context.SaveChanges();

			return new EmptyResult();
		}

		[HttpPost]
		[Authorize]
		public ActionResult SaveSite()
		{
			var id = Request["ItemId"];
			var name = Request["Name"];
			var municipality = Request["Municipality"];
			var state = Request["State"];
			var contactName = Request["ContactName"];
			var contactPhone = Request["ContactPhone"];
			var contactEmail = Request["ContactEmail"];

			if (id == "-1")
			{
				// Insert
				var context = new UsersContext();
				SiteProfile sp = new SiteProfile() { Name = name, Municipality = municipality, State = state, ContactName = contactName, ContactPhone = contactPhone, ContactEmail = contactEmail };
				context.SiteProfiles.Add(sp);
				context.SaveChanges();

				id = sp.Id.ToString();
			}
			else
			{
				// Update
				// Read on options for updating here: http://stackoverflow.com/questions/15336248/entity-framework-5-updating-a-record
				var context = new UsersContext();
				int iid = Convert.ToInt32(id);
				SiteProfile sp = context.SiteProfiles.Find(iid);
				sp.Name = name;
				sp.Municipality = municipality;
				sp.State = state;
				sp.ContactName = contactName;
				sp.ContactPhone = contactPhone;
				sp.ContactEmail = contactEmail;
				context.SaveChanges();
			}

			return Json(new { ItemId = id });
		}

		// ========= USER MANAGEMENT ==============

		[Authorize]
		public ActionResult UserManagement()
		{
			var context = new UsersContext();
			var userInfo = context.UserInfo;
			List<UserItem> users = new List<UserItem>();

			foreach (UserInfo ui in userInfo)
			{
				users.Add(new UserItem() { Id = ui.Id, FirstName = ui.FirstName, LastName = ui.LastName, Email = ui.Email });
			}

			HtmlHelper helper = this.GetHtmlHelper();

			// Initialize grid metadata
			GridMetadata gridMetadata = new GridMetadata();
			gridMetadata.AddColumn("col2", "firstname", ColumnMetadata.Control.TextBox, "FirstName", "First Name", "20");
			gridMetadata.AddColumn("col3", "lastname", ColumnMetadata.Control.TextBox, "LastName", "Last Name", "20");
			gridMetadata.AddColumn("col4", "email", ColumnMetadata.Control.TextBox, "Email", "Email", "20");

			// Initialize grid
			WebGrid grid = new WebGrid(users, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "setArrows");

			List<WebGridColumn> columnSet = gridMetadata.GetColumnSet(helper, grid);

			// Look at EngineContext.Current.Resolve<GenericController>() to be able to call the controller methods directly.
			// See http://stackoverflow.com/questions/5960664/calling-a-method-in-the-controller

			ViewBag.Grid = grid;
			ViewBag.ColumnSet = columnSet;
			ViewBag.InlineNewRow = gridMetadata.GetInlineNewRow();
			ViewBag.EditGetters = gridMetadata.EditGetters();
			ViewBag.DisplaySetters = gridMetadata.DisplaySetters();
			ViewBag.PostPath = "/Admin/SaveUser";
			ViewBag.DeletePath = "/Admin/DeleteUser";
			ViewBag.PostbackParams = gridMetadata.PostbackParams();
			ViewBag.PopulateDropDownLists = gridMetadata.PopulateDropDownLists();

			return View();
		}

		[HttpPost]
		[Authorize]
		public ActionResult DeleteUser()
		{
			int id = Convert.ToInt32(Request["ItemId"]);
			var context = new UsersContext();
			UserInfo ui = context.UserInfo.Find(id);
/*
			// Delete the user info record if it exists.
			UserProfile up = context.UserProfiles.SingleOrDefault(u => u.UserName == ui.Email);

			if (up != null)
			{
				context.UserProfiles.Remove(up);
			}
*/
			context.UserInfo.Remove(ui);
			context.SaveChanges();

			return new EmptyResult();
		}

		[HttpPost]
		[Authorize]
		public ActionResult SaveUser()
		{
			var id = Request["ItemId"];
			var firstName = Request["FirstName"];
			var lastName = Request["LastName"];
			var email = Request["Email"];

			if (id == "-1")
			{
				// Insert
				var context = new UsersContext();

				// Save the user info.  We must have a selected site!
				string token = Guid.NewGuid().ToString();
				UserInfo sp = new UserInfo() { FirstName=firstName, LastName=lastName, Email=email, RegistrationToken=token, Activated=false, SiteId = Convert.ToInt32(Session["SiteId"]) };
				context.UserInfo.Add(sp);
				context.SaveChanges();

				// Lastly, email the user a registration link.
				EmailRegistrationLink(email, firstName, token);

				id = sp.Id.ToString();
			}
			else
			{
				// Update
				// Read on options for updating here: http://stackoverflow.com/questions/15336248/entity-framework-5-updating-a-record
				var context = new UsersContext();
				int iid = Convert.ToInt32(id);
				UserInfo ui = context.UserInfo.Find(iid);
				ui.FirstName = firstName;
				ui.LastName = lastName;
				ui.Email = email;
				context.SaveChanges();
			}

			return Json(new { ItemId = id });
		}

		protected void EmailRegistrationLink(string email, string firstName, string token)
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

 			string html = Razor.Parse(template, new { FirstName = firstName, Token = token });
//			string html = "";

			WebMail.Send(to: email, subject: "Welcome to DPWorks", body: html);
		}
    }
}
