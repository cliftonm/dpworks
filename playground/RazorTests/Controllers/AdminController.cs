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
		// [AllowAnonymous]
		[Authorize]
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

		// ========= UNIT MANAGEMENT ==============

		[Authorize]
		public ActionResult UnitManagement()
		{
			// var db = Database.Open("DefaultConnection");
			var context = new UsersContext();
			var units = context.Units;

			HtmlHelper helper = this.GetHtmlHelper();

			// Initialize grid metadata
			GridMetadata gridMetadata = new GridMetadata();
			gridMetadata.AddColumn("col5", "abbr", ColumnMetadata.Control.TextBox, "Abbr", "Abbr", "20");
			gridMetadata.AddColumn("col5", "name", ColumnMetadata.Control.TextBox, "Name", "Name", "15");
			gridMetadata.AddColumn("col5", "description", ColumnMetadata.Control.TextBox, "Description", "Description", "5");

			// Initialize grid
			WebGrid grid = new WebGrid(units, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "setArrows");

			List<WebGridColumn> columnSet = gridMetadata.GetColumnSet(helper, grid);

			// Look at EngineContext.Current.Resolve<GenericController>() to be able to call the controller methods directly.
			// See http://stackoverflow.com/questions/5960664/calling-a-method-in-the-controller

			ViewBag.Grid = grid;
			ViewBag.ColumnSet = columnSet;
			ViewBag.InlineNewRow = gridMetadata.GetInlineNewRow();
			ViewBag.EditGetters = gridMetadata.EditGetters();
			ViewBag.DisplaySetters = gridMetadata.DisplaySetters();
			ViewBag.PostPath = "/Admin/SaveUnit";
			ViewBag.DeletePath = "/Admin/DeleteUnit";
			ViewBag.PostbackParams = gridMetadata.PostbackParams();
			ViewBag.PopulateDropDownLists = gridMetadata.PopulateDropDownLists();

			return View();
		}

		[HttpPost]
		[Authorize]
		public ActionResult DeleteUnit()
		{
			int id = Convert.ToInt32(Request["ItemId"]);
			var context = new UsersContext();
			Unit sp = context.Units.Find(id);

			context.Units.Remove(sp);
			context.SaveChanges();

			return new EmptyResult();
		}

		[HttpPost]
		[Authorize]
		public ActionResult SaveUnit()
		{
			var id = Request["ItemId"];
			var abbr = Request["Abbr"];
			var name = Request["Name"];
			var descr = Request["Description"];

			if (id == "-1")
			{
				// Insert
				var context = new UsersContext();
				Unit sp = new Unit() { Name = name, Abbr = abbr, Description = descr, SiteId = Convert.ToInt32(Session["SiteId"]) };
				context.Units.Add(sp);
				context.SaveChanges();

				id = sp.Id.ToString();
			}
			else
			{
				// Update
				// Read on options for updating here: http://stackoverflow.com/questions/15336248/entity-framework-5-updating-a-record
				var context = new UsersContext();
				int iid = Convert.ToInt32(id);
				Unit sp = context.Units.Find(iid);
				sp.Name = name;
				sp.Abbr = abbr;
				sp.Description = descr;
				context.SaveChanges();
			}

			return Json(new { ItemId = id });
		}

		[Authorize]
		public ActionResult GetUnits()
		{
			var context = new UsersContext();
			// We need more explicit names so "Id" and "Name" doesn't interfere with other fields with id's if "id" and "name"
			var units = context.Units.OrderBy(u => u.Name).Select(u => new { UnitId = u.Id, UnitName = u.Name });		
			var jsonResult = System.Web.Helpers.Json.Encode(units);
			return Json(jsonResult, JsonRequestBehavior.AllowGet);
		}

		// ========= LABOR RATE MANAGEMENT ==============

		[Authorize]
		public ActionResult LaborRateManagement()
		{
			// var db = Database.Open("DefaultConnection");
			var context = new UsersContext();
			var laborRate = context.LaborRates;

			HtmlHelper helper = this.GetHtmlHelper();

			// Initialize grid metadata
			GridMetadata gridMetadata = new GridMetadata();
			gridMetadata.AddColumn("col5", "position", ColumnMetadata.Control.TextBox, "Position", "Position", "20");
			gridMetadata.AddColumn("col5", "hourlyrate", ColumnMetadata.Control.TextBox, "HourlyRate", "Hourly Rate", "15");

			// Initialize grid
			WebGrid grid = new WebGrid(laborRate, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "setArrows");

			List<WebGridColumn> columnSet = gridMetadata.GetColumnSet(helper, grid);

			// Look at EngineContext.Current.Resolve<GenericController>() to be able to call the controller methods directly.
			// See http://stackoverflow.com/questions/5960664/calling-a-method-in-the-controller

			ViewBag.Grid = grid;
			ViewBag.ColumnSet = columnSet;
			ViewBag.InlineNewRow = gridMetadata.GetInlineNewRow();
			ViewBag.EditGetters = gridMetadata.EditGetters();
			ViewBag.DisplaySetters = gridMetadata.DisplaySetters();
			ViewBag.PostPath = "/Admin/SaveLaborRate";
			ViewBag.DeletePath = "/Admin/DeleteLaborRate";
			ViewBag.PostbackParams = gridMetadata.PostbackParams();
			ViewBag.PopulateDropDownLists = gridMetadata.PopulateDropDownLists();

			return View();
		}

		[HttpPost]
		[Authorize]
		public ActionResult DeleteLaborRate()
		{
			int id = Convert.ToInt32(Request["ItemId"]);
			var context = new UsersContext();
			LaborRate sp = context.LaborRates.Find(id);

			context.LaborRates.Remove(sp);
			context.SaveChanges();

			return new EmptyResult();
		}

		[HttpPost]
		[Authorize]
		public ActionResult SaveLaborRate()
		{
			var id = Request["ItemId"];
			var position = Request["Position"];
			var hourlyRate = Request["HourlyRate"];

			if (id == "-1")
			{
				// Insert
				var context = new UsersContext();
				LaborRate sp = new LaborRate() {Position = position, HourlyRate = Convert.ToDecimal(hourlyRate), SiteId = Convert.ToInt32(Session["SiteId"]) };
				context.LaborRates.Add(sp);
				context.SaveChanges();

				id = sp.Id.ToString();
			}
			else
			{
				// Update
				// Read on options for updating here: http://stackoverflow.com/questions/15336248/entity-framework-5-updating-a-record
				var context = new UsersContext();
				int iid = Convert.ToInt32(id);
				LaborRate sp = context.LaborRates.Find(iid);
				sp.Position = position;
				sp.HourlyRate = Convert.ToDecimal(hourlyRate);
				context.SaveChanges();
			}

			return Json(new { ItemId = id });
		}

		// ========= EQUIPMENT MANAGEMENT ==============

		[Authorize]
		public ActionResult EquipmentManagement()
		{
			// var db = Database.Open("DefaultConnection");
			var context = new UsersContext();
			var equip = context.Equipment;

			HtmlHelper helper = this.GetHtmlHelper();

			// Initialize grid metadata
			GridMetadata gridMetadata = new GridMetadata();
			gridMetadata.AddColumn("col5", "name", ColumnMetadata.Control.TextBox, "Name", "Item Name", "20");
			gridMetadata.AddColumn("col5", "hourlyrate", ColumnMetadata.Control.TextBox, "HourlyRate", "Hourly Rate", "15");

			// Initialize grid
			WebGrid grid = new WebGrid(equip, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "setArrows");

			List<WebGridColumn> columnSet = gridMetadata.GetColumnSet(helper, grid);

			// Look at EngineContext.Current.Resolve<GenericController>() to be able to call the controller methods directly.
			// See http://stackoverflow.com/questions/5960664/calling-a-method-in-the-controller

			ViewBag.Grid = grid;
			ViewBag.ColumnSet = columnSet;
			ViewBag.InlineNewRow = gridMetadata.GetInlineNewRow();
			ViewBag.EditGetters = gridMetadata.EditGetters();
			ViewBag.DisplaySetters = gridMetadata.DisplaySetters();
			ViewBag.PostPath = "/Admin/SaveEquipment";
			ViewBag.DeletePath = "/Admin/DeleteEquipment";
			ViewBag.PostbackParams = gridMetadata.PostbackParams();
			ViewBag.PopulateDropDownLists = gridMetadata.PopulateDropDownLists();

			return View();
		}

		[HttpPost]
		[Authorize]
		public ActionResult DeleteEquipment()
		{
			int id = Convert.ToInt32(Request["ItemId"]);
			var context = new UsersContext();
			Equipment sp = context.Equipment.Find(id);

			context.Equipment.Remove(sp);
			context.SaveChanges();

			return new EmptyResult();
		}

		[HttpPost]
		[Authorize]
		public ActionResult SaveEquipment()
		{
			var id = Request["ItemId"];
			var name = Request["Name"];
			var hourlyRate = Request["HourlyRate"];

			if (id == "-1")
			{
				// Insert
				var context = new UsersContext();
				Equipment sp = new Equipment() { Name = name, HourlyRate = Convert.ToDecimal(hourlyRate), SiteId = Convert.ToInt32(Session["SiteId"]) };
				context.Equipment.Add(sp);
				context.SaveChanges();

				id = sp.Id.ToString();
			}
			else
			{
				// Update
				// Read on options for updating here: http://stackoverflow.com/questions/15336248/entity-framework-5-updating-a-record
				var context = new UsersContext();
				int iid = Convert.ToInt32(id);
				Equipment sp = context.Equipment.Find(iid);
				sp.Name = name;
				sp.HourlyRate = Convert.ToDecimal(hourlyRate);
				context.SaveChanges();
			}

			return Json(new { ItemId = id });
		}

		// ========= MATERIAL MANAGEMENT ==============

		[Authorize]
		public ActionResult MaterialManagement()
		{
			// var db = Database.Open("DefaultConnection");
			var context = new UsersContext();
			var materialTable = context.Materials;

			// A "code" view.  Gross.
			var material = materialTable.Select(m => new { Id = m.Id, SiteId = m.SiteId, Name = m.Name, UnitId = m.UnitId, UnitCost = m.UnitCost, UnitName = m.Unit.Name });
			// Another "code" view because we expect "Value" and "Text" for the selection list field names.  We *could* fix that ourselves.
			var units = context.Units.Select(u => new { Value = u.Id, Text = u.Name });		

			HtmlHelper helper = this.GetHtmlHelper();

			// Initialize grid metadata
			GridMetadata gridMetadata = new GridMetadata();
			gridMetadata.AddColumn("col5", "name", ColumnMetadata.Control.TextBox, "Name", "Item Name", "20");
			gridMetadata.AddColumn("col5", "UnitId", ColumnMetadata.Control.DropDownList, "UnitId", "UnitName", units, "UnitId", "unitname", "UnitId", "UnitName", "-- Select Unit --", "/Admin/GetUnits");
			gridMetadata.AddColumn("col5", "unitcost", ColumnMetadata.Control.TextBox, "UnitCost", "Unit Cost", "15");

			// Initialize grid
			WebGrid grid = new WebGrid(material, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "setArrows");

			List<WebGridColumn> columnSet = gridMetadata.GetColumnSet(helper, grid);

			// Look at EngineContext.Current.Resolve<GenericController>() to be able to call the controller methods directly.
			// See http://stackoverflow.com/questions/5960664/calling-a-method-in-the-controller

			ViewBag.Grid = grid;
			ViewBag.ColumnSet = columnSet;
			ViewBag.InlineNewRow = gridMetadata.GetInlineNewRow();
			ViewBag.EditGetters = gridMetadata.EditGetters();
			ViewBag.DisplaySetters = gridMetadata.DisplaySetters();
			ViewBag.PostPath = "/Admin/SaveMaterial";
			ViewBag.DeletePath = "/Admin/DeleteMaterial";
			ViewBag.PostbackParams = gridMetadata.PostbackParams();
			ViewBag.PopulateDropDownLists = gridMetadata.PopulateDropDownLists();

			return View();
		}

		[HttpPost]
		[Authorize]
		public ActionResult DeleteMaterial()
		{
			int id = Convert.ToInt32(Request["ItemId"]);
			var context = new UsersContext();
			Material sp = context.Materials.Find(id);

			context.Materials.Remove(sp);
			context.SaveChanges();

			return new EmptyResult();
		}

		[HttpPost]
		[Authorize]
		public ActionResult SaveMaterial()
		{
			var id = Request["ItemId"];
			var name = Request["Name"];
			var unitid = Request["UnitId"];
			var unitCost = Request["UnitCost"];

			if (id == "-1")
			{
				// Insert
				var context = new UsersContext();
				Material sp = new Material() { Name = name, UnitId = Convert.ToInt32(unitid), UnitCost = Convert.ToDecimal(unitCost), SiteId = Convert.ToInt32(Session["SiteId"]) };
				context.Materials.Add(sp);
				context.SaveChanges();

				id = sp.Id.ToString();
			}
			else
			{
				// Update
				// Read on options for updating here: http://stackoverflow.com/questions/15336248/entity-framework-5-updating-a-record
				var context = new UsersContext();
				int iid = Convert.ToInt32(id);
				Material sp = context.Materials.Find(iid);
				sp.Name = name;
				sp.UnitCost = Convert.ToDecimal(unitCost);
				sp.UnitId = Convert.ToInt32(unitid);
				context.SaveChanges();
			}

			return Json(new { ItemId = id });
		}

	}
}
