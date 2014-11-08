using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RazorTests
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);

			routes.MapRoute(
				name: "ViewBook",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "ViewBook", action = "Index", id = UrlParameter.Optional }
			);

			routes.MapRoute(
				name: "RoleManagment",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Admin", action = "RoleManagement", id = UrlParameter.Optional }
			);

			//routes.MapRoute(
			//	name: "SaveBook",
			//	url: "{controller}/{action}/{id}",
			//	defaults: new { controller = "ViewBook", action = "SaveChanges", id = UrlParameter.Optional }
			//);
		}
	}
}