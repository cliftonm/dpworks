using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;

namespace dpworkswebsite
{
	public static class Utils
	{
		public static Dictionary<string, string> FixAnnoyingDataIssues(Dictionary<string, string> parms)
		{
			// Ridiculous behavior of the jqDataTable control when adding records.
			// The new Id value is actually in UID for new records.
			if (parms.ContainsKey("uid"))
			{
				parms["Id"] = parms["uid"];
			}

			// Annoying - we need to remove these fields.
			parms.Remove("uid");
			parms.Remove("_visible");

			Dictionary<string, string> parms2 = new Dictionary<string, string>();

			// Also annoying -- spaces have been encoded with a "+", so we have to put them back to spaces.
			// TODO: Can this be avoided by using JSON for the data?
			parms.ForEach(kvp => parms2[kvp.Key] = kvp.Value.Replace("+", " "));

			return parms2;
		}
	}
}
