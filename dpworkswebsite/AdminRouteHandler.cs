using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.WebServer;

namespace dpworkswebsite
{
	/// <summary>
	/// Page is accessible only to users with admin role.
	/// Also has base functionality of the AuthenticatedExpirable route handler.
	/// </summary>
	public class AdminRouteHandler : AuthenticatedExpirableRouteHandler
	{
		public AdminRouteHandler(Func<Session, Dictionary<string, string>, ResponsePacket> handler = null)
			: base(handler)
		{
		}

		public override ResponsePacket Handle(Session session, Dictionary<string, string> parms)
		{
			ResponsePacket ret;

			if (!session.IsAdmin())
			{
				ret = Server.Redirect(Server.onError(Server.ServerError.NotAuthorized));
			}
			else
			{
				ret = base.Handle(session, parms);
			}

			return ret;
		}
	}
}
