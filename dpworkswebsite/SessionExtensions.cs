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
		// Set the IsAdmin flag
		public static void IsAdmin(this Session session, bool isAdmin)
		{
			session["IsAdmin"] = isAdmin;
		}

		// Return true/false for whether the IsAdmin flag is set or not.
		public static bool IsAdmin(this Session session)
		{
			return session.GetObject<bool>("IsAdmin");
		}
	}
}
