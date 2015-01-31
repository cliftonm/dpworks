using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;
using Clifton.WebServer;

using dpworkswebsite.Controllers;

namespace dpworkswebsite
{
	class Program
	{
		static void Main(string[] args)
		{
			string websitePath = GetWebsitePath();
			Server.onError = ErrorHandler;

			// For testing, always authorized, never expired.
			Server.onRequest = (session, context) =>
			{
				session.Authorized = true;
				session.UpdateLastConnectionTime();
			};

			Server.postProcess = PostProcessor;

			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/", Handler = new AnonymousRouteHandler((session, parms) => Server.Redirect("/login")) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/index", Handler = new AnonymousRouteHandler((session, parms) => Server.Redirect("/login")) });
			Server.AddRoute(new Route() { Verb = Router.POST, Path = "/login", Handler = new AnonymousRouteHandler(LoginController.ValidateClient) });

			Server.Start(websitePath);
			System.Diagnostics.Process.Start("http://localhost");
			Console.ReadLine();
		}

		public static string PostProcessor(Session session, string html)
		{
			html = Server.DefaultPostProcess(session, html);

			html = html.Replace("@CurrentYear@", DateTime.Now.Year.ToString());

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
