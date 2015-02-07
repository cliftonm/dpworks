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

// TODO: The exception message should be logged / emailed and potentially should only be reported on screen in debug mode.

namespace dpworkswebsite.Controllers
{
	public class GenericTableController
	{
		/// <summary>
		/// This is the name of the jqWidget data table control, having nothing to do with back-end table names.
		/// </summary>
		public string DataTableName { get; set; }

		public ViewInfo View { get; set; }
		public string CallbackObjectName { get; set; }
		public Func<Session, SqlFragment> WhereClause { get; set; }
		public Func<Session, Dictionary<string, object>> AdditionalInsertParams {get;set;}

		public ResponsePacket GetRecords(Session session, Dictionary<string, object> kvParams)
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

		public ResponsePacket UpdateRecord(Session session, Dictionary<string, object> kvParams)
		{
			ResponsePacket resp = null;

			try
			{
				// Because of how jqxDataTable works:
				// In a data table with a DropDownList, the selected *display name* associated with the *lookup field name* is returned.
				// We need to convert this to the non-aliased master table FK ID and lookup the value from the FK table given the name.
				// ANNOYINGLY, THIS MEANS THAT THE NAME MUST ALSO BE UNIQUE!  This should be acceptable, if not even good practice, but it's still an annoying constraint.
				DbService db = new Services.DbService();
				FixupLookups(db, kvParams);
				kvParams = Utils.FixAnnoyingDataIssues(kvParams);
				db.Update(View, kvParams);
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("OK") };
			}
			catch (Exception ex)
			{
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("Error") };
				Console.WriteLine(ex.Message);
			}

			return resp;
		}

		public ResponsePacket InsertRecord(Session session, Dictionary<string, object> kvParams)
		{
			ResponsePacket resp = null;

			// TODO: This is very kludgy because of the way jqxDataTable works: it
			// wants us to add the record with no data, but we have non-nullable columns!
			// As a result, we let application define the default values for an "empty record insert."
			View.Fields.Where(f => !f.IsPK && !f.IsNullable && f.DefaultValue != null).ForEach(f => kvParams[f.Alias] = f.DefaultValue);
			// Replace any existing not nullable field with application-specific defined parameters.
			// Note that this is a function call, which allows for realtime value updates, as opposed to the default value above, 
			// which is a one-time assignment on initialization.
			AdditionalInsertParams.IfNotNull(p => p(session).ForEach(kvp => kvParams[kvp.Key]=kvp.Value));

			try
			{
				kvParams = Utils.FixAnnoyingDataIssues(kvParams);
				DbService db = new Services.DbService();
				decimal id = db.Insert(View, kvParams);
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(id.ToString()) };
			}
			catch (Exception ex)
			{
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("Error") };
				Console.WriteLine(ex.Message);
			}

			return resp;
		}

		public ResponsePacket DeleteRecord(Session session, Dictionary<string, object> kvParams)
		{
			ResponsePacket resp = null;

			try
			{
				kvParams = Utils.FixAnnoyingDataIssues(kvParams);
				DbService db = new Services.DbService();
				db.Delete(View, kvParams);
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("OK") };
			}
			catch (Exception ex)
			{
				resp = new ResponsePacket() { Data = Encoding.UTF8.GetBytes("Error") };
				Console.WriteLine(ex.Message);
			}

			return resp;
		}

		// TODO: Decouple this from jqWidgets
		public string Initialize(Session session, Dictionary<string, object> parms, string html)
		{
			int lookupCounter = 0;
			List<string> fields = new List<string>();
			List<string> columns = new List<string>();
			List<string> daLookups = new List<string>();
			View.Fields.ForEach(f => fields.Add("{ name: " + f.Alias.SingleQuote() + ", type: " + f.GetJavascriptType().SingleQuote() + "}"));
			View.Fields.Where(f => f.Visible).ForEach(f => 
				{
					if (!f.IsFK)
					{
						// The simpler case -- a straight forward mapping of the data field.
						columns.Add("{ text: " + f.Caption.SingleQuote() + 
							", dataField: " + f.Alias.SingleQuote() + 
							", width: " + f.Width.SingleQuote() + 
							"}\r\n");
					}
					else
					{
						++lookupCounter;
						List<string> lookupFields = new List<string>();

						// Let app handle exception if the view isn't registered.
						ViewInfo lookupViewInfo = ViewInfo.RegisteredViews[f.LookupInfo.ViewName];
						lookupViewInfo.Fields.ForEach(lvfi => lookupFields.Add(("name: " + lvfi.FieldName.SingleQuote() + ", type: " + lvfi.GetJavascriptType().SingleQuote()).CurlyBraces()));
						string lookupFieldStr = String.Join(",", lookupFields);

						string lookup = @"
							var lookup" + lookupCounter + @" =
							{
								datatype: 'json',
								datafields: [" + lookupFieldStr + @"],
								url: " + f.LookupInfo.Url.SingleQuote() + @",
								async: false
							};

							var daLookup" + lookupCounter + @" = new $.jqx.dataAdapter(lookup" + lookupCounter + @", { uniqueDataFields: ['Id'] });
							";

						daLookups.Add(lookup);
						// Hardcoded to make sure it works first.
						// See the example at the bottom of this file for what the generated columns looks like.
						columns.Add("{ text: " + f.Caption.SingleQuote() +
							", columntype: 'template'" +
							", dataField: " + f.Alias.SingleQuote() +
							", displayField: " + f.LookupInfo.ValueFieldName.SingleQuote() +	// This is the field in the FK table that is joined to the master table, and is a kludge because of how jqxDataTable with a jqxDropDownList works.
							", width: " + f.Width.SingleQuote() +
							", createEditor: function(row, cellvalue, editor, cellText, width, height) {editor.jqxDropDownList({source: daLookup" + lookupCounter + ", displayMember: " + f.LookupInfo.ValueFieldName.SingleQuote() + ", valueMember: " + f.LookupInfo.ValueFieldName.SingleQuote() + ", width: width, height: height});},\r\n" +
							"initEditor: function (row, cellvalue, editor, celltext, width, height) {editor.jqxDropDownList({ width: width, height: height });editor.val(cellvalue);},\r\n" +
							"getEditorValue: function (row, cellvalue, editor) {return editor.val();}}\r\n");
					}
				});

			// Create the initializers for the data when a record is added.
			List<string> initializers = new List<string>();

			View.Fields.Where(f => f.DefaultValue != null && !String.IsNullOrEmpty(f.DefaultValue.ToString())).ForEach(f => AddInitializer(initializers, f.DataType, f.Alias, f.DefaultValue));

			// Add the computed values to the initializer list.
			View.Fields.Where(f => f.ComputedValue != null).ForEach(f => AddInitializer(initializers, f.DataType, f.Alias, f.ComputedValue(session)));

			string fieldArray = String.Join(",", fields);
			string columnsArray = String.Join(",", columns);
			string initMap = String.Join(",", initializers);
			string js = String.Join("\r\n", daLookups);

			js = js + @"            
            var rowIndex;           // The index of the selected row.
            var recordID;           // Our ID field for the selected record.
            var tableNameID = " + DataTableName.SingleQuote() + @";
            var tableDataUrl = " + (CallbackObjectName + "List").SingleQuote() + @";
            var addRecordUrl = '/add" + CallbackObjectName + @"?__CSRFToken__=@CSRFValue@';
            var updateRecordUrl = '/update" + CallbackObjectName + @"?__CSRFToken__=@CSRFValue@';
            var deleteRecordUrl = '/delete" + CallbackObjectName + @"?__CSRFToken__=@CSRFValue@';
            var addRecordInitializers = {" + initMap + @"};
            var fields = [" + fieldArray + @"];
            var columns = [" + columnsArray + @"]";

			html = html.Replace("// @InitRecordInfo@", js);

			return html;
		}

		private void FixupLookups(DbService db, Dictionary<string, object> kvParams)
		{
			// Find all lookup fields:
			View.Fields.Where(f => f.IsFK).ForEach(f =>
				{
					object val;

					if (kvParams.TryGetValue(f.LookupInfo.ValueFieldName, out val))
					{
						// Remove the *value field* name
						kvParams.Remove(f.LookupInfo.ValueFieldName);
						// Replace with the non-aliased *FK field* name.
						// TODO: This doesn't handle multiple columns referencing the same FK table.
						// Now, lookup the value in the FK table.  Sigh, this requires a database query.  If the jqxDataTable / jqxDropDownList worked correctly with ID's, this wouldn't be necessary!
						object id = db.QueryScalar("select " + f.LookupInfo.IdFieldName + " from " + f.LookupInfo.ViewName + " where " + f.LookupInfo.ValueFieldName + "=@val", new Dictionary<string, object>() { { "@val", val } });
						// TODO: If the FK column allows nulls, then we can leave this null, otherwise a value is required, and we use -1 for "no lookup found" - the jqxDropDownList's "Please Choose:" option.
						kvParams[f.Alias] = (id == null) ? -1 : id;
					}
				});
		}

		/// <summary>
		/// Add an initializer with the appropriate data type / constant correction for Javascript.
		/// </summary>
		private void AddInitializer(List<string> initializers, Type dataType, string alias, object val)
		{
			if (dataType == typeof(string))
			{
				initializers.Add(alias + ": '" + val + "'");
			}
			else if (dataType == typeof(bool))
			{
				initializers.Add(alias + ": " + (((bool)val) ? "true" : "false"));
			}
			else if (dataType == typeof(DateTime))
			{
				initializers.Add(alias + ": '" + val + "'");
			}
			else
			{
				// Without single quotes.
				initializers.Add(alias + ": " + val);
			}
		}
	}
}

/* Example of the FK column drop down list:

[{ text: 'Name', dataField: 'MaterialName', width: '15%' },
        {
            text: 'Unit', columntype: 'template', dataField: 'UnitId', displayField: 'Abbr', width: '20%',
            createEditor: function (row, cellvalue, editor, cellText, width, height) {
                editor.jqxDropDownList({ source: daUnitList, displayMember: 'Abbr', valueMember: 'Abbr', width: width, height: height });
            },
            initEditor: function (row, cellvalue, editor, celltext, width, height) {
                editor.jqxDropDownList({ width: width, height: height });
                alert("cellvalue = " + cellvalue + "   celltext = " + celltext);
                editor.val(cellvalue);
            },
            getEditorValue: function (row, cellvalue, editor) {
                return editor.val();
            }
        },
        { text: 'Cost', dataField: 'MaterialUnitCost', width: '20%' }]

*/


// Example of how to set up the json data source for a lookup field.
/*

            var unitList =
            {
                datatype: "json",
                datafields: [
                    { name: 'Id', type: 'number' },
                    { name: 'Abbr', type: 'string' },
                    { name: 'Name', type: 'string' }
                ],
                url: "/unitlist",
                async: false
            };

            var daUnitList = new $.jqx.dataAdapter(unitList, { uniqueDataFields: ["Id"] });


*/