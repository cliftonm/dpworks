using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Clifton.WebServer;

using dpworkswebsite.Models;

namespace dpworkswebsite.Controllers
{
	public static class SiteController
	{
		public static ResponsePacket GetSiteList(Session session, Dictionary<string, string> kvParams)
		{
			DataTable dt = SiteModel.LoadSites();
			string json = JsonConvert.SerializeObject(dt);

			return new ResponsePacket() { Data = Encoding.UTF8.GetBytes(json), ContentType = "application/json" };
		}

		public static ResponsePacket UpdateSite(Session session, Dictionary<string, string> kvParams)
		{
			ResponsePacket resp = null;

			try
			{
				decimal id = SiteModel.InsertOrUpdate(kvParams);
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(id.ToString()) };
			}
			catch (Exception ex)
			{
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("Error: " + ex.Message) };
			}

			return resp;
		}
		
		public static ResponsePacket AddSite(Session session, Dictionary<string, string> kvParams)
		{
			ResponsePacket resp = null;
			kvParams["Name"] = "";
			kvParams["Municipality"] = "";
			kvParams["State"] = "";
			kvParams["ContactName"] = "";
			kvParams["ContactPhone"] = "";
			kvParams["ContactEmail"] = "";

			try
			{
				decimal id = SiteModel.Insert(kvParams);
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(id.ToString()) };
			}
			catch (Exception ex)
			{
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("Error: " + ex.Message) };
			}

			return resp;
		}
		
		public static ResponsePacket DeleteSite(Session session, Dictionary<string, string> kvParams)
		{
			ResponsePacket resp = null;

			try
			{
				SiteModel.Delete(kvParams);
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("OK") };
			}
			catch (Exception ex)
			{
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("Error: " + ex.Message) };
			}

			return resp;
		}
	}
}
