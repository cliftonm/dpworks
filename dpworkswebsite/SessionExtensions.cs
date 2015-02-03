using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.WebServer;

namespace dpworkswebsite
{
	public static class SessionExtensions
	{
		private const string strIsAdmin = "IsAdmin";
		private const string strSiteID = "SiteID";

		// Set the IsAdmin flag
		public static void IsAdmin(this Session session, bool isAdmin)
		{
			session[strIsAdmin] = isAdmin;
		}

		// Return true/false for whether the IsAdmin flag is set or not.
		public static bool IsAdmin(this Session session)
		{
			return session.GetObject<bool>(strIsAdmin);
		}

		public static void SiteID(this Session session, decimal id)
		{
			session[strSiteID] = id;
		}

		public static decimal SiteID(this Session session)
		{
			return session.GetObject<decimal>(strSiteID);
		}
	}
}
