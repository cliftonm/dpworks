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
		private const string strEstimateID = "EstimateID";
		private const string strSiteName = "SiteName";
		private const string strIsAuthenticated = "IsAuthenticated";

		// IsAdmin

		public static void IsAdmin(this Session session, bool isAdmin)
		{
			session[strIsAdmin] = isAdmin;
		}

		// Return true/false for whether the IsAdmin flag is set or not.
		public static bool IsAdmin(this Session session)
		{
			return session.GetObject<bool>(strIsAdmin);
		}

		// SiteID

		public static void SiteID(this Session session, decimal id)
		{
			session[strSiteID] = id;
		}

		public static decimal SiteID(this Session session)
		{
			return session.GetObject<decimal>(strSiteID);
		}

		// EstimateID

		public static void EstimateID(this Session session, decimal id)
		{
			session[strEstimateID] = id;
		}

		public static decimal EstimateID(this Session session)
		{
			return session.GetObject<decimal>(strEstimateID);
		}

		// SiteName

		public static void SiteName(this Session session, string name)
		{
			session[strSiteName] = name;
		}

		public static string SiteName(this Session session)
		{
			return session.GetObject<string>(strSiteName);
		}
	}
}
