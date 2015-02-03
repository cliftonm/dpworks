using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Clifton.WebServer;

using dpworkswebsite.Services;

namespace dpworkswebsite.Controllers
{
	public static class LoginController
	{
		/// <summary>
		/// Validate the client's username and password.
		/// </summary>
		public static ResponsePacket ValidateClient(Session session, Dictionary<string, object> data)
		{
			ResponsePacket ret = null;

			// TODO: For now, we validate the user based on their email address and a fixed password.
			// TODO: We will fix this when we implement the registration system and get email working.

			DbService db = new DbService();
			// Notice that we ignore the site id, as we assume that email addresses are unique across sites.
			Dictionary<string, string> record = db.QuerySingleRow("select SiteId, PasswordHash from [User] where Email=@email", new Dictionary<string, object>() { { "@email", data["email"].ToString() } });

			if ( (record == null) || (data["password"].ToString() != "dpworks"))
			{
				session.Authenticated = false;
				ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("Error") };
			}
			else 
			{
				session.Authenticated = true;
				session.IsAdmin(true);
				session.SiteID(Convert.ToInt32(record["SiteId"]));
				ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("OK") };
			}

			return ret;
		}

		public static ResponsePacket Logout(Session session, Dictionary<string, object> data)
		{
			session.Authenticated = false;
			session.IsAdmin(false);
			
			return new ResponsePacket() { Redirect = "/" };
		}
	}
}
