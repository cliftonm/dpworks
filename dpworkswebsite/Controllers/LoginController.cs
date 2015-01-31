using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.WebServer;

namespace dpworkswebsite.Controllers
{
	public static class LoginController
	{
		/// <summary>
		/// Validate the client's username and password.
		/// </summary>
		public static ResponsePacket ValidateClient(Session session, Dictionary<string, string> data)
		{
			ResponsePacket ret = null;

			// TODO: Do real login validation
			if ((data["username"] == "admin") && (data["password"] == "admin"))
			{
				// When validated, the user is authorized to view pages requiring authorization.
				session.Authorized = true;
				// Redirect to the menu page.
				ret = Server.Redirect("/menu");
			}
			else
			{
				session.Authorized = false;
			}

			return ret;
		}
	}
}
