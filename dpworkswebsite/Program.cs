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
			GenericTableController equipment = InitializeEquipmentController();
			GenericTableController laborRates = InitializeLaborRatesController();
			GenericTableController estimates = InitializeEstimateController();
			GenericTableController estimateMaterials = InitializeEstimateMaterialsController();

			// These views are used as lookups and must be registered so we can find the view when creating the data source in Javascript.
			sites.View.Register();
			units.View.Register();
			material.View.Register();
			laborRates.View.Register();
			equipment.View.Register();

			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/login", Handler = new AnonymousRouteHandler(LoginController.ValidateClient) });
			Server.AddRoute(new Route() { Verb = Router.GET, Path = "/logout", Handler = new AnonymousRouteHandler(LoginController.Logout) });

			// ---------- EDIT MATERIAL ESTIMATE ROUTING ----------

			// TODO: Make this an authenticated route when ready.
			// Custom routing for the three grids on the edit estimate route
			Server.AddRoute(new Route()
			{
				Verb = Router.GET,
				Path = "/editEstimate",
				Handler = new AuthenticatedExpirableRouteHandler((session, parms)=>
				{
					// Set the estimate ID.
					session.EstimateID(Convert.ToInt32(parms["id"]));
					return null;
				}),
				PostProcess = (session, parms, html) =>
				{
					// Build the html for the material, equipment, and labor grids.
					html = estimateMaterials.Initialize(session, parms, html);

					return html;
				}
			});

			AddAjaxCrudOperations("materialEstimate", estimateMaterials);

			// -----------------------------

			// SITE SELECTION ROUTING

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
			AddAdminRoutes("/sites", "site", sites);
			AddAdminRoutes("/users", "user", users);

			// TODO: These simply require authentication.
			AddRoutes("/units", "unit", units);
			AddRoutes("/materials", "material", material);
			AddRoutes("/laborRates", "laborrate", laborRates);
			AddRoutes("/estimates", "estimate", estimates);
			AddRoutes("/equipment", "equipment", equipment);

			Server.Start(websitePath);
			System.Diagnostics.Process.Start("http://localhost/editEstimate?id=17");
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
					// How to read the next line: Replace pageHtml's @TableEditor@ with customContent.
					pageHtml = Merge(customContent, pageHtml, "@TableEditor@");
				}

				string layoutHtml = File.ReadAllText(websitePath + "\\Pages\\_layout.html");
				// How to read the next line: Replace layoutHtml's @Content@ with pageHtml.
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

		/// <summary>
		/// Add routes requiring admin, authenticated, expirable handler.
		/// </summary>
		private static void AddAdminRoutes(string url, string callbackObjectName, GenericTableController controller)
		{
			Server.AddRoute(new Route() { Verb = Router.GET, Path = url, Handler = new AdminRouteHandler(), PostProcess = controller.Initialize });

			// AJAX requests / postbacks
			Server.AddRoute(new Route() { Verb = Router.GET, Path = "/" + callbackObjectName + "list", Handler = new AdminRouteHandler(controller.GetRecords) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/update" + callbackObjectName, Handler = new AdminRouteHandler(controller.UpdateRecord) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/add" + callbackObjectName, Handler = new AdminRouteHandler(controller.InsertRecord) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/delete" + callbackObjectName, Handler = new AdminRouteHandler(controller.DeleteRecord) });
		}

		// Add routes with authenticated, expirable handler.
		private static void AddRoutes(string url, string callbackObjectName, GenericTableController controller)
		{
			Server.AddRoute(new Route() { Verb = Router.GET, Path = url, Handler = new AdminRouteHandler(), PostProcess = controller.Initialize });
			AddAjaxCrudOperations(callbackObjectName, controller);
		}

		private static void AddAjaxCrudOperations(string callbackObjectName, GenericTableController controller)
		{
			// AJAX requests / postbacks
			Server.AddRoute(new Route() { Verb = Router.GET, Path = "/" + callbackObjectName + "list", Handler = new AuthenticatedExpirableRouteHandler(controller.GetRecords) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/update" + callbackObjectName, Handler = new AuthenticatedExpirableRouteHandler(controller.UpdateRecord) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/add" + callbackObjectName, Handler = new AuthenticatedExpirableRouteHandler(controller.InsertRecord) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/delete" + callbackObjectName, Handler = new AuthenticatedExpirableRouteHandler(controller.DeleteRecord) });
		}

		// SITES

		private static GenericTableController InitializeSiteController()
		{
			GenericTableController controller = new GenericTableController()
			{
				DataTableName = "#siteTable",
				CallbackObjectName = "site",
				View = InitializeSiteView(),
				WhereClause = (Session session) => new SqlFragment() { Sql = "where Id > -1" },
			};

			return controller;
		}

		private static ViewInfo InitializeSiteView()
		{
			ViewInfo view = new ViewInfo() { Name="SiteProfile", Tables = new List<string> () {"SiteProfile"} };
			view.Fields.Add(new ViewFieldInfo() { TableName = "SiteProfile", FieldName = "Id", Visible = false, DataType=typeof(decimal), IsPK=true });
			view.Fields.Add(new ViewFieldInfo() { TableName = "SiteProfile", FieldName = "Name", Caption = "Name", Width = "15%", DataType = typeof(string), DefaultValue = "new site" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Municipality", TableName = "SiteProfile", FieldName = "Municipality", Width = "15%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "State", TableName = "SiteProfile", FieldName = "State", Width = "5%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Contact Name", TableName = "SiteProfile", FieldName = "ContactName", Width = "20%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Contact Phone", TableName = "SiteProfile", FieldName = "ContactPhone", Width = "15%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Contact Email", TableName = "SiteProfile", FieldName = "ContactEmail", Width = "30%", DataType = typeof(string), DefaultValue = "" });
			
			return view;
		}

		// UNITS

		private static GenericTableController InitializeUnitsController()
		{
			GenericTableController controller = new GenericTableController()
			{
				DataTableName = "#unitTable",
				CallbackObjectName = "unit",
				View = InitializeUnitsView(),
				WhereClause = (Session session) => new SqlFragment() { Sql = "where SiteId = @SiteId", Parameters = GetSiteIdAsParam(session) },
				AdditionalInsertParams = (Session session) => GetSiteIdAsParam(session),
			};

			return controller;
		}

		private static ViewInfo InitializeUnitsView()
		{
			ViewInfo view = new ViewInfo() { Name="Unit", Tables = {"Unit"} };
			view.Fields.Add(new ViewFieldInfo() { TableName = "Unit", FieldName = "Id", Visible = false, DataType = typeof(decimal), IsPK = true });
			view.Fields.Add(new ViewFieldInfo() { TableName = "Unit", FieldName = "SiteId", Visible = false, DataType = typeof(decimal) });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Abbreviation", TableName = "Unit", FieldName = "Abbr", Width = "15%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Name", TableName = "Unit", FieldName = "Name", Width = "15%", DataType = typeof(string), DefaultValue = "new unit" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Description", TableName = "Unit", FieldName = "Description", Width = "20%", DataType = typeof(string), DefaultValue = "" });

			return view;
		}

		// MATERIALS

		private static GenericTableController InitializeMaterialController()
		{
			GenericTableController controller = new GenericTableController()
			{
				DataTableName = "#materialTable",
				CallbackObjectName = "material",
				View = InitializeMaterialView(),
				// TODO: The hardcoded "a." in the where clause needs to be eliminated.  Figure out the best way to do this.
				WhereClause = (Session session) => new SqlFragment() { Sql = "where a.SiteId = @SiteId", Parameters = new Dictionary<string, object>() { { "@SiteId", session.SiteID() } } },
				AdditionalInsertParams = (Session session) => GetSiteIdAsParam(session),
			};

			return controller;
		}

		private static ViewInfo InitializeMaterialView()
		{
			ViewInfo view = new ViewInfo() { Name="Material", Tables = { "Material", "Unit" } };
			view.Fields.Add(new ViewFieldInfo() { TableName = "Material", FieldName = "Id", Visible = false, DataType = typeof(decimal), IsPK = true });
			view.Fields.Add(new ViewFieldInfo() { TableName = "Material", FieldName = "SiteId", Visible = false, DataType = typeof(decimal) });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Name", TableName = "Material", FieldName = "Name", Width = "15%", DataType = typeof(string), DefaultValue = "new item", Alias="MaterialName" });
			
			view.Fields.Add(new ViewFieldInfo() 
			{ 
				Caption = "Unit", 
				TableName = "Material", 
				FieldName = "UnitId", 
				Width = "20%", 
				DataType = typeof(decimal), 
				Alias = "MaterialUnitId", 
				LookupInfo = new LookupInfo() 
				{ 
					ViewName = "Unit", 
					IdFieldName = "Id", 
					ValueFieldName = "Abbr",
					Url="/unitList",
				}, 
				DefaultValue = -1 
			});

			view.Fields.Add(new ViewFieldInfo() { Caption = "Cost", TableName = "Material", FieldName = "UnitCost", Width = "20%", DataType = typeof(decimal), DefaultValue = 0, Alias="MaterialUnitCost" });
			view.Fields.Add(new ViewFieldInfo() { TableName = "Unit", FieldName = "Abbr", Visible = false, DataType = typeof(string) });

			return view;
		}

		// EQUIPMENT

		private static GenericTableController InitializeEquipmentController()
		{
			GenericTableController controller = new GenericTableController()
			{
				DataTableName = "#equipmentTable",
				CallbackObjectName = "equipment",
				View = InitializeEquipmentView(),
				// TODO: The hardcoded "a." in the where clause needs to be eliminated.  Figure out the best way to do this.
				WhereClause = (Session session) => new SqlFragment() { Sql = "where a.SiteId = @SiteId", Parameters = new Dictionary<string, object>() { { "@SiteId", session.SiteID() } } },
				AdditionalInsertParams = (Session session) => GetSiteIdAsParam(session),
			};

			return controller;
		}

		private static ViewInfo InitializeEquipmentView()
		{
			ViewInfo view = new ViewInfo() { Name = "Equipment", Tables = { "Equipment" } };
			view.Fields.Add(new ViewFieldInfo() { TableName = "Equipment", FieldName = "Id", Visible = false, DataType = typeof(decimal), IsPK = true });
			view.Fields.Add(new ViewFieldInfo() { TableName = "Equipment", FieldName = "SiteId", Visible = false, DataType = typeof(decimal) });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Name", TableName = "Equipment", FieldName = "Name", Width = "35%", DataType = typeof(string), DefaultValue = "new item" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Hourly Rate", TableName = "Equipment", FieldName = "HourlyRate", Width = "20%", DataType = typeof(decimal), DefaultValue = 0 });

			return view;
		}

		// LABOR RATES

		private static GenericTableController InitializeLaborRatesController()
		{
			GenericTableController controller = new GenericTableController()
			{
				DataTableName = "#laborRatesTable",
				CallbackObjectName = "laborrate",
				View = InitializeLaborRatesView(),
				WhereClause = (Session session) => new SqlFragment() { Sql = "where SiteId = @SiteId", Parameters = new Dictionary<string, object>() { { "@SiteId", session.SiteID() } } },
				AdditionalInsertParams = (Session session) => GetSiteIdAsParam(session),
			};

			return controller;
		}

		private static ViewInfo InitializeLaborRatesView()
		{
			ViewInfo view = new ViewInfo() { Name="LaborRate", Tables = {"LaborRate"} };
			view.Fields.Add(new ViewFieldInfo() { TableName = "LaborRate", FieldName = "Id", Visible = false, DataType = typeof(decimal), IsPK = true });
			view.Fields.Add(new ViewFieldInfo() { TableName = "LaborRate", FieldName = "SiteId", Visible = false, DataType = typeof(decimal) });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Position", TableName = "LaborRate", FieldName = "Position", Width = "15%", DataType = typeof(string), DefaultValue = "new position" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Hourly Rate", TableName = "LaborRate", FieldName = "HourlyRate", Width = "20%", DataType = typeof(decimal), DefaultValue = 0 });

			return view;
		}

		// USERS

		private static GenericTableController InitializeUsersController()
		{
			GenericTableController controller = new GenericTableController()
			{
				DataTableName = "#userTable",
				CallbackObjectName = "user",
				View = InitializeUsersView(),
				WhereClause = (Session session) => new SqlFragment() { Sql = "where SiteId = @SiteId", Parameters = new Dictionary<string, object>() { { "@SiteId", session.SiteID() } } },
				AdditionalInsertParams = (Session session) => GetSiteIdAsParam(session),
			};

			return controller;
		}

		private static ViewInfo InitializeUsersView()
		{
			ViewInfo view = new ViewInfo() { Name="User", Tables = {"User"} };

			// Hidden fields.
			view.Fields.Add(new ViewFieldInfo() { TableName = "User", FieldName = "Id", Visible = false, DataType = typeof(decimal), IsPK = true });
			view.Fields.Add(new ViewFieldInfo() { TableName = "User", FieldName = "SiteId", Visible = false, DataType = typeof(decimal) });
			view.Fields.Add(new ViewFieldInfo() { TableName = "User", FieldName = "RegistrationToken", Visible = false, DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { TableName = "User", FieldName = "Activated", Visible = false, DataType = typeof(bool), DefaultValue = false });
			view.Fields.Add(new ViewFieldInfo() { TableName = "User", FieldName = "PasswordHash", Visible = false, DataType = typeof(string), DefaultValue = "" });

			view.Fields.Add(new ViewFieldInfo() { Caption = "First Name", TableName = "User", FieldName = "FirstName", Width = "15%", DataType = typeof(string), DefaultValue = "new user" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Last Name", TableName = "User", FieldName = "LastName", Width = "15%", DataType = typeof(string), DefaultValue = "" });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Email", TableName = "User", FieldName = "Email", Width = "25%", DataType = typeof(string), DefaultValue = "" });

			return view;
		}

		// ESTIMATE

		private static GenericTableController InitializeEstimateController()
		{
			GenericTableController controller = new GenericTableController()
			{
				DataTableName = "#estimateTable",
				CallbackObjectName = "estimate",
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
			ViewInfo view = new ViewInfo() { Name="Estimate", Tables = {"Estimate"} };

			// Hidden fields.
			view.Fields.Add(new ViewFieldInfo() { TableName = "Estimate", FieldName = "Id", Visible = false, DataType = typeof(decimal), IsPK = true });
			view.Fields.Add(new ViewFieldInfo() { TableName = "Estimate", FieldName = "SiteId", Visible = false, DataType = typeof(decimal) });
			view.Fields.Add(new ViewFieldInfo()
			{
				Caption = "Date",
				TableName = "Estimate",
				FieldName = "EstimateDate",
				DataType = typeof(DateTime),
				Width = "20%",
				SqlFormat = "CONVERT(VARCHAR(10), EstimateDate, 101)",
				ComputedValue = (session) => DateTime.Now.ToString("MM/dd/yyyy"),
			});
			view.Fields.Add(new ViewFieldInfo() { Caption = "Road", TableName = "Estimate", FieldName = "Road", DataType = typeof(string), Width = "80%", DefaultValue = "" });

			return view;
		}

		// ESTIMATE MATERIALS

		private static GenericTableController InitializeEstimateMaterialsController()
		{
			GenericTableController controller = new GenericTableController()
			{
				DataTableName = "#materialEstimateTable",
				CallbackObjectName = "materialEstimate",
				View = InitializeEstimateMaterialsView(),
				WhereClause = (Session session) => new SqlFragment() { Sql = "where EstimateId = @EstimateId", Parameters = new Dictionary<string, object>() { { "@EstimateId", session.EstimateID() } } },
				AdditionalInsertParams = (Session session) => GetEstimateIdAsParam(session),
			};

			return controller;
		}

		private static ViewInfo InitializeEstimateMaterialsView()
		{
			ViewInfo view = new ViewInfo() { Name="EstimateMaterial", Tables = {"EstimateMaterial", "Material"} };
			view.Fields.Add(new ViewFieldInfo() { TableName = "EstimateMaterial", FieldName = "Id", Visible = false, DataType = typeof(decimal), IsPK = true });
			view.Fields.Add(new ViewFieldInfo() { TableName = "EstimateMaterial", FieldName = "EstimateId", Visible = false, DataType = typeof(decimal) });
			
			view.Fields.Add(new ViewFieldInfo()
			{
				Caption="Material",
				TableName = "EstimateMaterial",
				FieldName = "MaterialId",
				Width = "25%",
				DataType = typeof(decimal),
				Alias = "EstimateMaterialId",
				LookupInfo = new LookupInfo()
				{
					ViewName = "Material",
					IdFieldName = "Id",
					ValueFieldName = "Name",
					AliasedFieldName = "MaterialName", // This is an aliased name from the material list.  Remember this is a view joining Material and Unit
					Url = "/materialList",
				},
				DefaultValue = -1
			});

			view.Fields.Add(new ViewFieldInfo() { Caption = "Quantity", TableName = "EstimateMaterial", FieldName = "Quantity", Width = "10%", DataType = typeof(decimal), DefaultValue = 0 });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Unit Cost", TableName = "EstimateMaterial", FieldName = "UnitCost", Width = "15%", DataType = typeof(decimal), DefaultValue = 0 });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Total", TableName = "EstimateMaterial", FieldName = "Total", Width = "15%", DataType = typeof(decimal), IsSqlComputed = true });

			// Notice that we alias the Material.Name field to match the AliasedFieldName in the LookupInfo for MaterialID, which is calling the Material view and where Material.Name is also aliased to MaterialName.
			// TODO: There might be a required coupling between the aliased names that should exist.  Check this out at some point.
			view.Fields.Add(new ViewFieldInfo() { TableName = "Material", FieldName = "Name", Alias = "MaterialName", Visible = false, DataType = typeof(string) });

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
