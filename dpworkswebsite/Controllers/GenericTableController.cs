using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Clifton.ExtensionMethods;
using Clifton.WebServer;

using dpworkswebsite.Models;
using dpworkswebsite.Services;

namespace dpworkswebsite.Controllers
{
	public class GenericTableController
	{
		public string TableName { get; set; }
		public string InitField { get; set; }
		public string InitValue { get; set; }
		public ViewInfo View { get; set; }
		public string CallbackObjectName { get; set; }
		public Func<Session, SqlFragment> WhereClause { get; set; }

		public ResponsePacket GetRecords(Session session, Dictionary<string, string> kvParams)
		{
			DbService db = new Services.DbService();

			// Get the where clause and associated parameters for querying the view.
			string sqlWhere = null;
			Dictionary<string, object> parms = null;
			WhereClause.IfNotNull(w =>
				{
					SqlFragment fragment = w(session);
					sqlWhere = fragment.Sql;
					parms = fragment.Parameters;
				});

			DataTable dt = db.Query(View, sqlWhere, parms);
			string json = JsonConvert.SerializeObject(dt);

			return new ResponsePacket() { Data = Encoding.UTF8.GetBytes(json), ContentType = "application/json" };
		}

		public ResponsePacket UpdateRecord(Session session, Dictionary<string, string> kvParams)
		{
			ResponsePacket resp = null;

			try
			{
				kvParams = Utils.FixAnnoyingDataIssues(kvParams);
				DbService db = new Services.DbService();
				decimal id = db.InsertOrUpdate(View.TableName, kvParams);
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(id.ToString()) };
			}
			catch (Exception ex)
			{
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("Error: " + ex.Message) };
			}

			return resp;
		}

		public ResponsePacket AddRecord(Session session, Dictionary<string, string> kvParams)
		{
			ResponsePacket resp = null;

			// TODO: This is very kludgy because of the way jqxDataTable works: it
			// wants us to add the record with no data, but we have non-nullable columns!
			// We also should initialize based on the data type!
			View.Fields.Where(f => !f.IsPK && !f.IsNullable).ForEach(f => kvParams[f.FieldName] = "");

			try
			{
				kvParams = Utils.FixAnnoyingDataIssues(kvParams);
				DbService db = new Services.DbService();
				decimal id = db.Insert(View.TableName, kvParams);
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(id.ToString()) };
			}
			catch (Exception ex)
			{
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("Error: " + ex.Message) };
			}

			return resp;
		}

		public ResponsePacket DeleteRecord(Session session, Dictionary<string, string> kvParams)
		{
			ResponsePacket resp = null;

			try
			{
				kvParams = Utils.FixAnnoyingDataIssues(kvParams);
				DbService db = new Services.DbService();
				db.Delete(View.TableName, kvParams);
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("OK") };
			}
			catch (Exception ex)
			{
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("Error: " + ex.Message) };
			}

			return resp;
		}

		public string Initialize(Session session, Dictionary<string, string> parms, string html)
		{
			List<string> fields = new List<string>();
			List<string> columns = new List<string>();
			View.Fields.ForEach(f => fields.Add("{ name: " + f.FieldName.SingleQuote() + ", type: " + f.GetJavascriptType().SingleQuote() + "}"));
			View.Fields.Where(f => f.Visible).ForEach(f => columns.Add("{ text: " + f.Caption.SingleQuote() + ", dataField: " + f.FieldName.SingleQuote() + ", width: " + f.Width.SingleQuote() + "}"));
			string fieldArray = String.Join(",", fields);
			string columnsArray = String.Join(",", columns);

			string js = @"            
            var rowIndex;           // The index of the selected row.
            var recordID;           // Our ID field for the selected record.
            var tableNameID = " + TableName.SingleQuote() + @";
            var tableDataUrl = " + (CallbackObjectName + "List").SingleQuote() + @";
            var addRecordUrl = '/add" + CallbackObjectName + @"?__CSRFToken__=@CSRFValue@';
            var updateRecordUrl = '/update" + CallbackObjectName + @"?__CSRFToken__=@CSRFValue@';
            var deleteRecordUrl = '/delete" + CallbackObjectName + @"?__CSRFToken__=@CSRFValue@';
            var addRecordInitField = " + InitField.SingleQuote() + @";
            var addRecordInitValue = " + InitValue.SingleQuote() + @";
            var fields = [" + fieldArray + @"];
            var columns = [" + columnsArray + @"]";

			html = html.Replace("// @InitRecordInfo@", js);

			return html;
		}
	}
}
