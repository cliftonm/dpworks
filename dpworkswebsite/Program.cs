using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;
using Clifton.WebServer;

using dpworkswebsite.Controllers;
using dpworkswebsite.Models;
using dpworkswebsite.Services;

// Book notes:
// Handling fonts
// Handling ~
// Default layout page
// Logging
// Session management (discard old sessions)
// AJAX post response is text?  not Json formatted data?  Oh wait, if specifying json content, the content must be formatted correctly!?  See SiteController.UpdateSite
// The different postback formats: JSON, key-value pairs, what else???
// Handling CSRF in not GET requests
// Handling CSRF in AJAX requests

namespace dpworkswebsite
{
	class Program
	{
		static string websitePath;

		static void Main(string[] args)
		{
#if RunOnLeftMonitor
			// For my home development system.
			Utils.SetConsoleWindowPosition(-1150, 110);
#endif
			websitePath = GetWebsitePath();
			Server.onError = ErrorHandler;

			// ======= DEVELOPER MODE ========
			// For testing, always authorized, never expired.
			Server.onRequest = (session, context) =>
			{
				session.Authenticated = true;
				session.UpdateLastConnectionTime();
				session.IsAdmin(true);

				// Initialize the site if it hasn't been assigned.
				if (session.SiteID() == 0)
				{
					session.SiteID(1);
					SetSiteName(session);
				}
			};
			// ======= DEVELOPER MODE ========

			Server.postProcess = PostProcessor;

			GenericTableController sites = InitializeSiteController();
			GenericTableController users = InitializeUsersController();
			GenericTableController units = InitializeUnitsController();
			GenericTableController material = InitializeMaterialController();
			GenericTableController laborRates = InitializeLaborRatesController();
			GenericTableController estimates = InitializeEstimateController();
			GenericTableController estimateMaterials = InitializeEstimateMaterialsController();

			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/login", Handler = new AnonymousRouteHandler(LoginController.ValidateClient) });
			Server.AddRoute(new Route() { Verb = Router.GET, Path = "/logout", Handler = new AnonymousRouteHandler(LoginController.Logout) });

			// TODO: Make this an authenticated route when ready.
			Server.AddRoute(new Route() { Verb = Router.GET, Path = "/newEstimate", Handler = new AnonymousRouteHandler() });

			Server.AddRoute(new Route()
			{
				Verb = Router.POST,
				Path = "/selectSite",
				Handler = new AdminRouteHandler((session, kvparms) =>
					{
						// Set the site ID when the admin changes the selected site.
						session.SiteID(Convert.ToDecimal(kvparms["siteID"]));
						SetSiteName(session);
						// Return the site name so that the UI can update the site name in the header.
						return new ResponsePacket() { Data = Encoding.UTF8.GetBytes(session.SiteName()) };
					})
			});

			// These require admin authorization
			AddRoutes("/sites", "site", sites);
			AddRoutes("/users", "user", users);

			// TODO: These simply require authentication.
			AddRoutes("/units", "unit", units);
			AddRoutes("/materials", "material", material);
			AddRoutes("/laborRates", "laborrate", laborRates);
			AddRoutes("/estimates", "estimate", estimates);

			Server.Start(websitePath);
			System.Diagnostics.Process.Start("http://localhost/estimates");
			Console.ReadLine();
		}

		public static string PostProcessor(Session session, string fileName, string pageHtml)
		{
			pageHtml = Server.DefaultPostProcess(session, fileName, pageHtml);

			// Here we inject the content into the layout page!

			if (fileName.RightOfRightmostOf("\\")[0] != '_')		// Skip sub-components of the HTML.
			{
				if (pageHtml.Contains("@TableEditor@"))
				{
					string customContent = File.ReadAllText(websitePath + "\\Pages\\_tableEditor.html");
					pageHtml = pageHtml.Replace("@TableEditor@", customContent);
				}

				string layoutHtml = File.ReadAllText(websitePath + "\\Pages\\_layout.html");
				pageHtml = Merge(pageHtml, layoutHtml, "@Content@");
			}

			pageHtml = pageHtml.Replace("@CurrentYear@", DateTime.Now.Year.ToString());
			pageHtml = pageHtml.Replace("$IsAdmin", session.IsAdmin() ? "true" : "false");
			pageHtml = pageHtml.Replace("$IsAuthenticated", session.Authenticated ? "true" : "false");
			pageHtml = pageHtml.Replace("$SiteID", session.SiteID().ToString());
			pageHtml = pageHtml.Replace("$SiteName", session.SiteName());

			return pageHtml;
		}

		private static string Merge(string pageHtml, string layoutHtml, string tag)
		{
			string pageHead = pageHtml.Between("<head>", "</head>");	// The the HEAD content in the page we're loading.
			string pageBody = pageHtml.Between("<body>", "</body>");	// The the BODY content in the page we're loading.

			// Now merge the two.  The head gets merged in to the existing head, and the body replaces @Content@.  Both without their respective tags.
			layoutHtml = layoutHtml.LeftOf("</head>") + pageHead + "</head>" + layoutHtml.RightOf("</head>");

			if (String.IsNullOrEmpty(pageBody))
			{
				// There's no body tag, so just replace @Content@ with whatever text is.
				pageHtml = layoutHtml.Replace(tag, pageHtml);
			}
			else
			{
				pageHtml = layoutHtml.Replace(tag, pageBody);
			}

			return pageHtml;
		}

		public static string GetWebsitePath()
		{
			// Path of our exe.
			string websitePath = Assembly.GetExecutingAssembly().Location;
			websitePath = websitePath.LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\") + "\\Website";

			return websitePath;
		}

		public static string ErrorHandler(Server.ServerError error)
		{
			string ret = null;

			switch (error)
			{
				case Server.ServerError.ExpiredSession:
					ret = "/ErrorPages/expiredSession.html";
					break;
				case Server.ServerError.FileNotFound:
					ret = "/ErrorPages/fileNotFound.html";
					break;
				case Server.ServerError.NotAuthorized:
					ret = "/ErrorPages/notAuthorized.html";
					break;
				case Server.ServerError.PageNotFound:
					ret = "/ErrorPages/pageNotFound.html";
					break;
				case Server.ServerError.ServerError:
					ret = "/ErrorPages/serverError.html";
					break;
				case Server.ServerError.UnknownType:
					ret = "/ErrorPages/unknownType.html";
					break;
				case Server.ServerError.ValidationError:
					ret = "/ErrorPages/validationError.html";
					break;
			}

			return ret;
		}

		private static void AddRoutes(string url, string callbackObjectName, GenericTableController controller)
		{
			Server.AddRoute(new Route() { Verb = Router.GET, Path = url, Handler = new AdminRouteHandler(), PostProcess = controller.Initialize });

			// AJAX requests / postbacks
			Server.AddRoute(new Route() { Verb = Router.GET, Path = "/" + callbackObjectName + "list", Handler = new AdminRouteHandler(controller.GetRecords) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/update" + callbackObjectName, Handler = new AdminRouteHandler(controller.UpdateRecord) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/add" + callbackObjectName, Handler = new AdminRouteHandler(controller.InsertRecord) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/delete" + callbackObjectName, Handler = new AdminRouteHandler(controller.DeleteRecord) });
		}

		// SITES

		private static GenericTableController InitializeSiteController()
		{
			GenericTableController controller = new GenericTableController()
			{
				TableName = "#siteTable",
				CallbackObjectName = "site",
				InitField = "Name",
				InitValue = "new site",
				View = InitializeSiteView(),
			};

			return controller;
		}

		private static ViewInfo InitializeSiteView()
		{
			ViewInfo view = new ViewInfo() { TableName = "SiteProfile" };
			view.Fields.Add(new ViewFieldInfo() { FieldName = "Id", Visible = false, DataType=typeof(decimal), IsPK=true });
			view.Fields.Add(new ViewFieldInfo() { FieldName = "Name", Caption = "Name", Width = "15%", DataType=typeof(string), DefaultValue=""});
			view.Fields.Add(new ViewFieldInfo() { Caption = "Municipality", FieldName = "Municipality", Width = "15%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "State", FieldName = "State", Width = "5%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Contact Name", FieldName = "ContactName", Width = "20%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Contact Phone", FieldName = "ContactPhone", Width = "15%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Contact Email", FieldName = "ContactEmail", Width = "30%", DataType = typeof(string), DefaultValue = "" });

			return view;
		}

		// UNITS

		private static GenericTableController InitializeUnitsController()
		{
			GenericTableController controller = new GenericTableController()
			{
				TableName = "#unitTable",
				CallbackObjectName = "unit",
				InitField = "Name",
				InitValue = "new unit",
				View = InitializeUnitsView(),
				WhereClause = (Session session) => new SqlFragment() { Sql = "where SiteId = @SiteId", Parameters = GetSiteIdAsParam(session) },
				AdditionalInsertParams = (Session session) => GetSiteIdAsParam(session),
			};

			return controller;
		}

		private static ViewInfo InitializeUnitsView()
		{
			ViewInfo view = new ViewInfo() { TableName = "Unit" };
			view.Fields.Add(new ViewFieldInfo() { FieldName = "Id", Visible = false, DataType = typeof(decimal), IsPK = true });
			view.Fields.Add(new ViewFieldInfo() { FieldName = "SiteId", Visible = false, DataType = typeof(decimal) });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Abbreviation", FieldName = "Abbr", Width = "15%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Name", FieldName = "Name", Width = "15%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Description", FieldName = "Description", Width = "20%", DataType = typeof(string), DefaultValue = "" });

			return view;
		}

		// MATERIALS

		private static GenericTableController InitializeMaterialController()
		{
			GenericTableController controller = new GenericTableController()
			{
				TableName = "#materialTable",
				CallbackObjectName = "material",
				InitField = "Name",
				InitValue = "new item",
				View = InitializeMaterialView(),
				WhereClause = (Session session) => new SqlFragment() { Sql = "where SiteId = @SiteId", Parameters = new Dictionary<string, object>() { { "@SiteId", session.SiteID() } } },
				AdditionalInsertParams = (Session session) => GetSiteIdAsParam(session),
			};

			return controller;
		}

		private static ViewInfo InitializeMaterialView()
		{
			ViewInfo view = new ViewInfo() { TableName = "Material" };
			view.Fields.Add(new ViewFieldInfo() { FieldName = "Id", Visible = false, DataType = typeof(decimal), IsPK = true });
			view.Fields.Add(new ViewFieldInfo() { FieldName = "SiteId", Visible = false, DataType = typeof(decimal) });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Name", FieldName = "Name", Width = "15%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Unit", FieldName = "UnitId", Width = "20%", DataType = typeof(decimal), LookupInfo = new LookupInfo() { TableName = "Unit", IdFieldName = "Id", ValueFieldName = "Abbr" }, DefaultValue=-1 });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Cost", FieldName = "UnitCost", Width = "20%", DataType = typeof(decimal), DefaultValue=0 });

			return view;
		}

		// LABOR RATES

		private static GenericTableController InitializeLaborRatesController()
		{
			GenericTableController controller = new GenericTableController()
			{
				TableName = "#laborRatesTable",
				CallbackObjectName = "laborrate",
				InitField = "Position",
				InitValue = "new position",
				View = InitializeLaborRatesView(),
				WhereClause = (Session session) => new SqlFragment() { Sql = "where SiteId = @SiteId", Parameters = new Dictionary<string, object>() { { "@SiteId", session.SiteID() } } },
				AdditionalInsertParams = (Session session) => GetSiteIdAsParam(session),
			};

			return controller;
		}

		private static ViewInfo InitializeLaborRatesView()
		{
			ViewInfo view = new ViewInfo() { TableName = "LaborRate" };
			view.Fields.Add(new ViewFieldInfo() { FieldName = "Id", Visible = false, DataType = typeof(decimal), IsPK = true });
			view.Fields.Add(new ViewFieldInfo() { FieldName = "SiteId", Visible = false, DataType = typeof(decimal) });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Position", FieldName = "Position", Width = "15%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Hourly Rate", FieldName = "HourlyRate", Width = "20%", DataType = typeof(decimal), DefaultValue=0 });

			return view;
		}

		// USERS

		private static GenericTableController InitializeUsersController()
		{
			GenericTableController controller = new GenericTableController()
			{
				TableName = "#userTable",
				CallbackObjectName = "user",
				InitField = "FirstName",
				InitValue = "new user",
				View = InitializeUsersView(),
				WhereClause = (Session session) => new SqlFragment() { Sql = "where SiteId = @SiteId", Parameters = new Dictionary<string, object>() { { "@SiteId", session.SiteID() } } },
				AdditionalInsertParams = (Session session) => GetSiteIdAsParam(session),
			};

			return controller;
		}

		private static ViewInfo InitializeUsersView()
		{
			ViewInfo view = new ViewInfo() { TableName = "User" };

			// Hidden fields.
			view.Fields.Add(new ViewFieldInfo() { FieldName = "Id", Visible = false, DataType = typeof(decimal), IsPK = true });
			view.Fields.Add(new ViewFieldInfo() { FieldName = "SiteId", Visible = false, DataType = typeof(decimal) });
			view.Fields.Add(new ViewFieldInfo() { FieldName = "RegistrationToken", Visible = false, DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { FieldName = "Activated", Visible = false, DataType = typeof(bool), DefaultValue = false });
			view.Fields.Add(new ViewFieldInfo() { FieldName = "PasswordHash", Visible = false, DataType = typeof(string), DefaultValue = "" });

			view.Fields.Add(new ViewFieldInfo() { Caption = "First Name", FieldName = "FirstName", Width = "15%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Last Name", FieldName = "LastName", Width = "15%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Email", FieldName = "Email", Width = "25%", DataType = typeof(string), DefaultValue = "" });

			return view;
		}

		// ESTIMATE

		private static GenericTableController InitializeEstimateController()
		{
			GenericTableController controller = new GenericTableController()
			{
				TableName = "#estimateTable",
				CallbackObjectName = "estimate",
				InitField = "EstimateDate",
				InitValue = DateTime.Now.ToString("MM/dd/yyyy"),
				View = InitializeEstimateView(),
				WhereClause = (Session session) => new SqlFragment() { Sql = "where SiteId = @SiteId", Parameters = new Dictionary<string, object>() { { "@SiteId", session.SiteID() } } },
				AdditionalInsertParams = (Session session) => new Dictionary<string, object>() 
				{ 
					{ "SiteId", session.SiteID() } ,
					{ "EstimateDate", DateTime.Now.ToString("MM/dd/yyyy") },
				},
			};

			return controller;
		}

		private static ViewInfo InitializeEstimateView()
		{
			ViewInfo view = new ViewInfo() { TableName = "Estimate" };

			// Hidden fields.
			view.Fields.Add(new ViewFieldInfo() { FieldName = "Id", Visible = false, DataType = typeof(decimal), IsPK = true });
			view.Fields.Add(new ViewFieldInfo() { FieldName = "SiteId", Visible = false, DataType = typeof(decimal) });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Date", FieldName = "EstimateDate", DataType = typeof(DateTime), Width = "20%", SqlFormat = "CONVERT(VARCHAR(10), EstimateDate, 101)" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Road", FieldName = "Road", DataType = typeof(string), Width="80%", DefaultValue="" });

			return view;
		}

		// ESTIMATE MATERIALS

		private static GenericTableController InitializeEstimateMaterialsController()
		{
			GenericTableController controller = new GenericTableController()
			{
				TableName = "#materialEstimateTable",
				CallbackObjectName = "materialEstimate",
				InitField = "Quantity",
				InitValue = "0",
				View = InitializeEstimateMaterialsView(),
				WhereClause = (Session session) => new SqlFragment() { Sql = "where SiteId = @EstimateId", Parameters = new Dictionary<string, object>() { { "@EstimateId", session.EstimateID() } } },
				AdditionalInsertParams = (Session session) => GetEstimateIdAsParam(session),
			};

			return controller;
		}

		private static ViewInfo InitializeEstimateMaterialsView()
		{
			ViewInfo view = new ViewInfo() { TableName = "EstimateMaterial" };
			view.Fields.Add(new ViewFieldInfo() { FieldName = "Id", Visible = false, DataType = typeof(decimal), IsPK = true });
			view.Fields.Add(new ViewFieldInfo() { FieldName = "EstimateId", Visible = false, DataType = typeof(decimal) });
			view.Fields.Add(new ViewFieldInfo() { FieldName = "MaterialId", Width="25%", DataType = typeof(decimal), LookupInfo = new LookupInfo() { TableName = "Materials", IdFieldName = "Id", ValueFieldName = "Name" }, DefaultValue=-1 });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Quantity", FieldName = "Quantity", Width = "10%", DataType = typeof(decimal), DefaultValue = 0 });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Unit Cost", FieldName = "UnitCost", Width = "15%", DataType = typeof(decimal), DefaultValue = 0 });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Total", FieldName = "Total", Width = "15%", DataType = typeof(decimal), DefaultValue = 0 });

			return view;
		}

		// ----------------------- helpers --------------------------

		/// <summary>
		/// return the site ID in a key-value dictionary.
		/// </summary>
		private static Dictionary<string, object> GetSiteIdAsParam(Session session)
		{
			// Note we specify the column name, not an "@..." parameter name.
			return new Dictionary<string, object>() { { "SiteId", session.SiteID() } };
		}

		private static Dictionary<string, object> GetEstimateIdAsParam(Session session)
		{
			// Note we specify the column name, not an "@..." parameter name.
			return new Dictionary<string, object>() { { "EstimateId", session.EstimateID() } };
		}

		private static void SetSiteName(Session session)
		{
			DbService db = new DbService();
			object obj = db.QueryScalar("select name from siteprofile where id = @SiteId", new Dictionary<string, object>() { { "@SiteId", session.SiteID() } });
			obj.IfNotNull(name => session.SiteName(name.ToString()));
		}
	}
}
