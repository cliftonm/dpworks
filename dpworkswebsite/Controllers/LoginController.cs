using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Clifton.WebServer;

namespace dpworkswebsite.Controllers
{
	public static class LoginController
	{
		/// <summary>
		/// If the session is not authorized, redirect to the login page, otherwise perform the default action (by returning null here.)
		/// </summary>
		public static ResponsePacket ValidateSession(Session session, Dictionary<string, string> data)
		{
			ResponsePacket ret = null;

			if (!session.Authorized)
			{
				ret = new ResponsePacket() {Redirect="/login"};
			}

			return ret;
		}

		/// <summary>
		/// Validate the client's username and password.
		/// </summary>
		public static ResponsePacket ValidateClient(Session session, Dictionary<string, string> data)
		{
			ResponsePacket ret = null;

			// TODO: Do real login validation
			if ((data["email"] == "a@foo.com") && (data["password"] == "admin"))
			{
				// When validated, the user is authorized to view pages requiring authorization.
				session.Authorized = true;
				ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("OK") };
			}
			else
			{
				session.Authorized = false;
				ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("Error") };
			}

			return ret;
		}
	}
}
