using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;

using dpworkswebsite.Services;

namespace dpworkswebsite.Models
{
	public static class SiteModel
	{
		public static DataTable LoadSites()
		{
			DbService db = new Services.DbService();
			DataTable dt = db.Query("select * from SiteProfile");

			return dt;
		}

		public static decimal InsertOrUpdate(Dictionary<string, string> parms)
		{
			parms = Utils.FixAnnoyingDataIssues(parms);

			// Save it!
			DbService db = new Services.DbService();
			decimal id = db.InsertOrUpdate("SiteProfile", parms);

			return id;
		}

		public static void Update(Dictionary<string, string> parms)
		{
			parms = Utils.FixAnnoyingDataIssues(parms);

			// Save it!
			DbService db = new Services.DbService();
			db.Update("SiteProfile", parms);
		}

		/// <summary>
		/// Insert the record and return the ID.
		/// </summary>
		public static decimal Insert(Dictionary<string, string> parms)
		{
			parms = Utils.FixAnnoyingDataIssues(parms);

			DbService db = new Services.DbService();
			decimal id = db.Insert("SiteProfile", parms);

			return id;
		}

		public static void Delete(Dictionary<string, string> parms)
		{
			parms = Utils.FixAnnoyingDataIssues(parms);

			DbService db = new Services.DbService();
			db.Delete("SiteProfile", parms);
		}
	}
}
