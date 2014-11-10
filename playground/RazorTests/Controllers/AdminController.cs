using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Mvc.Html;			// needed for HtmlHelper class extensions.
using WebMatrix.Data;

using RazorTests.Models;

namespace RazorTests.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult RoleManagement()
        {
            return View();
        }

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
		public ActionResult DeleteSite()
		{
			var id = Request["ItemId"];

			return new EmptyResult();
		}

		[HttpPost]
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
    }
}
