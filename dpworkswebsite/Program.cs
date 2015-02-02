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
			Utils.SetConsoleWindowPosition(-1150, 110);
			websitePath = GetWebsitePath();
			Server.onError = ErrorHandler;

			// For testing, always authorized, never expired.
			Server.onRequest = (session, context) =>
			{
				session.Authorized = true;
				session.UpdateLastConnectionTime();
				session.IsAdmin(true);
			};

			Server.postProcess = PostProcessor;
			GenericTableController sites = new GenericTableController()
			{
				TableName = "#siteTable",
				CallbackObjectName = "site",
				InitField = "Name",
				InitValue = "new site",
				View = InitializeSiteView(),
			};

			//Server.AddRoute(new Route() { Verb = Router.GET, Path = "/", Handler = new AnonymousRouteHandler(LoginController.ValidateSession) });
			//Server.AddRoute(new Route() { Verb = Router.GET, Path = "/index", Handler = new AnonymousRouteHandler(LoginController.ValidateSession) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/login", Handler = new AnonymousRouteHandler(LoginController.ValidateClient) });

			AddRoutes("/sites", "site", sites);

			Server.Start(websitePath);
			System.Diagnostics.Process.Start("http://localhost/Sites");
			Console.ReadLine();
		}

		public static string PostProcessor(Session session, string fileName, string html)
		{
			html = Server.DefaultPostProcess(session, fileName, html);

			html = html.Replace("@CurrentYear@", DateTime.Now.Year.ToString());

			// Here we inject the content into the layout page!

			if (fileName.RightOfRightmostOf("\\")[0] != '_')		// Skip sub-components of the HTML.
			{
				string text = File.ReadAllText(websitePath + "\\Pages\\_layout.html");
				html = text.Replace("@Content@", html);
			}

			return html;
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

		private static ViewInfo InitializeSiteView()
		{
			ViewInfo view = new ViewInfo() { TableName = "SiteProfile"};
			view.Fields.Add(new ViewFieldInfo() { FieldName = "Id", Visible = false, DataType=typeof(int), IsPK=true });
			view.Fields.Add(new ViewFieldInfo() { FieldName = "Name", Caption = "Name", Width = "15%", DataType=typeof(string)});
			view.Fields.Add(new ViewFieldInfo() { Caption = "Municipality", FieldName = "Municipality", Width = "15%", DataType = typeof(string) });
			view.Fields.Add(new ViewFieldInfo() { Caption = "State", FieldName = "State", Width = "5%", DataType = typeof(string) });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Contact Name", FieldName = "ContactName", Width = "20%", DataType = typeof(string) });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Contact Phone", FieldName = "ContactPhone", Width = "15%", DataType = typeof(string) });
			view.Fields.Add(new ViewFieldInfo() { Caption = "Contact Email", FieldName = "ContactEmail", Width = "30%", DataType=typeof(string) });

			return view;
		}

		private static void AddRoutes(string url, string callbackObjectName, GenericTableController controller)
		{
			Server.AddRoute(new Route() { Verb = Router.GET, Path = url, Handler = new AdminRouteHandler(), PostProcess = controller.Initialize });

			// AJAX requests / postbacks
			Server.AddRoute(new Route() { Verb = Router.GET, Path = "/" + callbackObjectName + "list", Handler = new AdminRouteHandler(controller.GetRecords) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/update" + callbackObjectName, Handler = new AdminRouteHandler(controller.UpdateRecord) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/add" + callbackObjectName, Handler = new AdminRouteHandler(controller.AddRecord) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/delete" + callbackObjectName, Handler = new AdminRouteHandler(controller.DeleteRecord) });
		}
	}
}
