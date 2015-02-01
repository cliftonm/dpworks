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

// Book notes:
// Handling fonts
// Handling ~
// Default layout page
// Logging
// Session management (discard old sessions)
// AJAX post response is text?  not Json formatted data?  Oh wait, if specifying json content, the content must be formatted correctly!?  See SiteController.UpdateSite
// The different postback formats: JSON, key-value pairs, what else???

namespace dpworkswebsite
{
	class Program
	{
		static string websitePath;

		static void Main(string[] args)
		{
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

			//Server.AddRoute(new Route() { Verb = Router.GET, Path = "/", Handler = new AnonymousRouteHandler(LoginController.ValidateSession) });
			//Server.AddRoute(new Route() { Verb = Router.GET, Path = "/index", Handler = new AnonymousRouteHandler(LoginController.ValidateSession) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/login", Handler = new AnonymousRouteHandler(LoginController.ValidateClient) });
			Server.AddRoute(new Route() { Verb = Router.GET, Path = "/sites", Handler = new AdminRouteHandler() });

			// AJAX requests / postbacks
			Server.AddRoute(new Route() { Verb = Router.GET, Path = "/sitelist", Handler = new AdminRouteHandler(SiteController.GetSiteList) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/updatesite", Handler = new AdminRouteHandler(SiteController.UpdateSite) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/addsite", Handler = new AdminRouteHandler(SiteController.AddSite) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/deletesite", Handler = new AdminRouteHandler(SiteController.DeleteSite) });

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
	}
}
