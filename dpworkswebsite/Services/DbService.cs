using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;
using Clifton.ValueConverter;

using dpworkswebsite.Models;

namespace dpworkswebsite.Services
{
	/// <summary>
	/// Used to map properties in a class to database field names.
	/// </summary>
	public class DbFieldNameAttribute : Attribute
	{
		public string FieldName { get; protected set; }

		public DbFieldNameAttribute(string name)
		{
			FieldName = name;
		}
	}

	public class SqlFragment
	{
		public string Sql { get; set; }
		public Dictionary<string, object> Parameters { get; set; }

		public SqlFragment()
		{
			Parameters = new Dictionary<string, object>();
		}
	}

	public class DbService
	{
		protected List<string> tableAliases = Enumerable.Range('a', 26).Select(x => ((char)x).ToString()).ToList();

		protected string connectionString;

		public DbService()
		{
			// Initialize the connection string from the app.config file.
			connectionString = ConfigurationManager.ConnectionStrings["ems"].ConnectionString;
		}

		/// <summary>
		/// Return the collection of records in the specified view.
		/// </summary>
		public DataTable Query(ViewInfo view, string whereClause = null, Dictionary<string, object> parms = null)
		{
			DataTable dt = new DataTable();
			IDbConnection conn = OpenConnection();
			IDbCommand cmd = conn.CreateCommand();

			// Build the query:
			StringBuilder sb = new StringBuilder("select ");
			sb.Append(String.Join(", ", view.Fields.Select(f => f.SqlFormat == null ? tableAliases[view.Tables.IndexOf(f.TableName)] + "." + f.FieldName + " as " + f.Alias : f.SqlFormat + " as " + f.Alias)));
			sb.Append(" from ");
			sb.Append(view.Tables[0].Brackets() + tableAliases[0].Spaced());

			// Left join remaining tables -- we ignore the "from" table.
			// TODO: At some point, we should let the programmer define the join type.
			// TODO: We need a much more sophisticated table-join walker than what we implement here.  I've already written this code elsewhere, might see if it can be ported.
			view.Tables.Skip(1).ForEachWithIndex((tname, idx) =>
			{
				sb.Append("left join" + tname.Brackets().Spaced() + tableAliases[idx+1] + " on ");
				// Right now, we assume strictly joining with the "from" table, and we always assume "Id" as the PK field.  We need access to the full ViewInfo schema collection to do this right.
				sb.Append(tableAliases[idx+1]+".Id = a.");
				// Find the field in the view that joins to this table.
				// TODO: This does not handle joining twice to the same table using separate FK fields.
				ViewFieldInfo vfiMaster = view.Fields.Where(vfi => vfi.IsFK && vfi.LookupInfo.ViewName == tname).Single();
				string masterJoinedFieldName = vfiMaster.FieldName;
				sb.Append(masterJoinedFieldName + " ");
			});
			
			// Append any where clause
			whereClause.IfNotNull(w => sb.Append(whereClause.Spaced()));
			parms.IfNotNull(p => p.ForEach(parm => cmd.Parameters.Add(new SqlParameter(parm.Key, parm.Value))));

			// Get the data.
			cmd.CommandText = sb.ToString();
			Console.WriteLine(sb.ToString());
			SqlDataAdapter da = new SqlDataAdapter((SqlCommand)cmd);
			da.Fill(dt);
			CloseConnection(conn);

			return dt;
		}

		/// <summary>
		/// Return a DataTable populated from the specified SQL query string.
		/// </summary>
		public DataTable Query(string sql, Dictionary<string, object> parms = null)
		{
			// The connection cannot be persisted as everything in these calls must be thread safe.
			DataTable dt = new DataTable();
			IDbConnection conn = OpenConnection();
			IDbCommand cmd = conn.CreateCommand();
			cmd.CommandText = sql;
			parms.IfNotNull((p) => p.ForEach(kvp => cmd.Parameters.Add(new SqlParameter(kvp.Key, kvp.Value))));
			SqlDataAdapter da = new SqlDataAdapter((SqlCommand)cmd);
			da.Fill(dt);
			CloseConnection(conn);

			return dt;
		}

		/// <summary>
		/// Return the single field (or first field) specified in the query, as a string.
		/// DBNull is returned as null
		/// </summary>
		public object QueryScalar(string sql, Dictionary<string, object> parms = null)
		{
			// The connection cannot be persisted as everything in these calls must be thread safe.
			IDbConnection conn = OpenConnection();
			IDbCommand cmd = conn.CreateCommand();
			cmd.CommandText = sql;
			parms.IfNotNull((p) => p.ForEach(kvp => cmd.Parameters.Add(new SqlParameter(kvp.Key, kvp.Value))));
			object obj = cmd.ExecuteScalar();

			object ret = (obj == DBNull.Value ? null : obj);

			return ret;
		}


		/// <summary>
		/// Return a hash of column name and value for the one and only row. 
		/// Returns null if the row doesn't exist.
		/// </summary>
		public Dictionary<string, string> QuerySingleRow(string sql, Dictionary<string, object> parms = null)
		{
			Dictionary<string, string> ret = null;
			DataTable dt = Query(sql, parms);

			if (dt.Rows.Count == 1)
			{
				ret = new Dictionary<string, string>();
				foreach (DataColumn dc in dt.Columns)
				{
					ret[dc.ColumnName] = dt.Rows[0][dc].ToString();
				}
			}
			else if (dt.Rows.Count > 1)
			{
				Console.WriteLine(sql);
				throw new ApplicationException("Returned more than one row.");
			}

			return ret;
		}

		/// <summary>
		/// Execute the sql statement with the optional parameters.
		/// </summary>
		public void Execute(string sql, Dictionary<string, object> parms = null)
		{
			IDbConnection conn = OpenConnection();
			IDbCommand cmd = conn.CreateCommand();
			cmd.CommandText = sql;
			parms.IfNotNull((p) => p.ForEach(kvp => cmd.Parameters.Add(new SqlParameter(kvp.Key, kvp.Value))));
			cmd.ExecuteNonQuery();
			CloseConnection(conn);
		}

		// Deprecated.
		/*
		public decimal InsertOrUpdate(string tableName, Dictionary<string, object> parms, string idField = "ID")
		{
			decimal ret = -1;

			string pkField = idField.ToLower();

			// Get the primary key field and value.  We do a case-insensitve search but return the exact matching pkfield name.
			// Let the caller handle any exception.
			KeyValuePair<string, object> pkKvp = parms.Where(kvp => kvp.Key.ToLower() == pkField).SingleOrDefault();

			if (String.IsNullOrEmpty(pkKvp.Key))
			{
				ret = Insert(tableName, parms);
			}
			else
			{
				Update(tableName, parms);
			}

			return ret;
		}
		*/

		/// <summary>
		/// Insert records in all tables in the view.
		/// </summary>
		public decimal Insert(ViewInfo view, Dictionary<string, object> parms)
		{
			// TODO: Implement multi-table insert.  This needs to be smart, preventing joined table inserts when they're just FK helpers.  Needs to be configurable in the ViewInfo for the table collection.
			// TODO: If implementing true multi-table inserts, need to figure out how to return the ID's of all affected table records.
			string tableName = view.Tables[0];
			Dictionary<string, object> dealiasedParms = GetFieldNames(view, tableName, parms);
			decimal ret = Insert(tableName, dealiasedParms);

			return ret;
		}

		public decimal Insert(string tableName, Dictionary<string, object> parms)
		{
			// Get the connection and create the command:
			IDbConnection conn = OpenConnection();
			IDbCommand cmd = conn.CreateCommand();

			// Build the update statement:
			StringBuilder sql = new StringBuilder("insert into");
			sql.Append(tableName.Brackets().Spaced());
			sql.Append("(");
			sql.Append(String.Join(",", parms.Keys));
			sql.Append(") values (");
			List<string> parmList= new List<string>();

			parms.ForEachWithIndex((kvp, idx) =>
				{
					parmList.Add("@" + idx);
					// Load the parameters with their values.
					cmd.Parameters.Add(new SqlParameter("@" + idx, kvp.Value));
				});

			sql.Append(String.Join(",", parmList));
			sql.Append("); select SCOPE_IDENTITY()");	// See here: http://stackoverflow.com/questions/5228780/how-to-get-last-inserted-id
			cmd.CommandText = sql.ToString();

			Console.WriteLine(cmd.CommandText);

			// Execute:
			object oid = cmd.ExecuteScalar();
			decimal id = Convert.ToDecimal(oid);
			CloseConnection(conn);

			return id;
		}

		/// <summary>
		/// Handles updating a multi-table view.  
		/// </summary>
		public void Update(ViewInfo view, Dictionary<string, object> parms, string idField = "ID")
		{
			// TODO: Only the master table is currently updated.  Need full implementation to update all tables in the view.
			string tableName = view.Tables[0];
			parms = GetFieldNames(view, tableName, parms);
			Update(tableName, parms, idField);
		}

		/// <summary>
		/// Update a record given the parameters and ID field.
		/// The ID field will be removed from the updated field set.
		/// The parms must include the ID field.
		/// </summary>
		public void Update(string tableName, Dictionary<string, object> parms, string idField = "ID")
		{
			KeyValuePair<string, object> pkKvp = GetPK(parms, idField);
			string pkField = pkKvp.Key;
			object pkValue = pkKvp.Value;

			// Copy the dictionary, so we can remove the PK value from this list.
			Dictionary<string, object> internalParms = new Dictionary<string, object>(parms);
			internalParms.Remove(pkField);

			// Get the connection and create the command:
			IDbConnection conn = OpenConnection();
			IDbCommand cmd = conn.CreateCommand();

			// Build the update statement:
			StringBuilder sql = new StringBuilder("update");
			sql.Append(tableName.Brackets().Spaced());
			sql.Append("set ");
			List<string> fieldValues = new List<string>();
			
			internalParms.ForEachWithIndex((kvp, idx) => 
				{
					fieldValues.Add(kvp.Key + "=@" + idx);
					// Load the parameters with their values.
					cmd.Parameters.Add(new SqlParameter("@" + idx, kvp.Value));
				});

			sql.Append(String.Join(",", fieldValues));
			sql.Append(" where " + pkField + " = " + pkValue);
			cmd.CommandText = sql.ToString();

			Console.WriteLine(cmd.CommandText);

			// Execute:
			cmd.ExecuteNonQuery();
			CloseConnection(conn);
		}

		public void Delete(ViewInfo view, Dictionary<string, object> parms, string idField = "ID")
		{
			// TODO: Only the master table record is currently deleted.  Need full implementation to delete records across all tables in the view.
			// However, this also needs to be smart -- the view needs to specify whether joined tables are read-only.
			string tableName = view.Tables[0];
			parms = GetFieldNames(view, tableName, parms);
			Delete(tableName, parms, idField);
		}

		/// <summary>
		/// Deletes a record for the given PK value in the parms collection.
		/// We use the collection because it's convenient to pass in the entire set of record fields.
		/// </summary>
		public void Delete(string tableName, Dictionary<string, object> parms, string idField = "ID")
		{
			KeyValuePair<string, object> pkKvp = GetPK(parms, idField);
			string pkField = pkKvp.Key;
			object pkValue = pkKvp.Value;

			// Get the connection and create the command:
			IDbConnection conn = OpenConnection();
			IDbCommand cmd = conn.CreateCommand();

			cmd.CommandText = "delete from " + tableName.Brackets() + " where ID=@ID";
			cmd.Parameters.Add(new SqlParameter("@ID", pkValue));

			Console.WriteLine(cmd.CommandText);

			cmd.ExecuteNonQuery();
			CloseConnection(conn);
		}

		/// <summary>
		/// Using reflection, populate a list of the specified generic type from the DataTable.
		/// </summary>
		/// <typeparam name="T">The type to populate.</typeparam>
		/// <param name="dt">The source data table.</param>
		/// <returns>A list of instances of type T.</returns>
		public List<T> Populate<T>(DataTable dt) where T : new()
		{
			List<T> items = new List<T>();
			Dictionary<string, PropertyInfo> objectProperties = GetObjectFields(typeof(T));

			foreach (DataRow row in dt.Rows)
			{
				T item = new T();

				objectProperties.ForEach(kvp =>
				{
					object sourceValue = row[kvp.Key];
					object targetValue = Converter.Convert(sourceValue, kvp.Value.PropertyType);
					kvp.Value.SetValue(item, targetValue);
				});

				items.Add(item);
			}

			// Here's a more direct way of doing this in Linq
			// var data = from row in dt.AsEnumerable() select new WebServer.Models.Performer() { ID = Convert.ToInt32(row["id"]), Name = row["name"].ToString(), StageName = row["stagename"].ToString() };
			// return data.OfType<T>().ToList();

			return items;
		}

		/// <summary>
		/// Get all the fields in the model decorated with the DbFieldName attribute and their associated PropertyInfo objects.
		/// We call this once, rather than computing this every single time for each model instance that we need to populate from the row.
		/// </summary>
		protected Dictionary<string, PropertyInfo> GetObjectFields(Type t)
		{
			Dictionary<string, PropertyInfo> props = new Dictionary<string, PropertyInfo>();

			t.GetProperties().ForEach(prop =>
			{
				prop.GetCustomAttribute(typeof(DbFieldNameAttribute)).IfNotNull(attr =>
				{
					string fieldName = ((DbFieldNameAttribute)attr).FieldName;
					props[fieldName] = prop;
				});
			});

			return props;
		}

		protected IDbConnection OpenConnection()
		{
			IDbConnection conn = new SqlConnection(connectionString);
			conn.Open();

			return conn;
		}

		protected void CloseConnection(IDbConnection conn)
		{
			conn.Close();
			// Closing a connection doesn't necessarily close it, as .NET implements connection pooling.
		}

		protected KeyValuePair<string, object> GetPK(Dictionary<string, object> parms, string idField)
		{
			string pkField = idField.ToLower();

			// Get the primary key field and value.  We do a case-insensitve search but return the exact matching pkfield name.
			// Let the caller handle any exception.
			KeyValuePair<string, object> pkKvp = parms.Where(kvp => kvp.Key.ToLower() == pkField).Single();

			return pkKvp;
		}

		/// <summary>
		/// Returns a collection of field-value pairs for the specified table.  The field names are non-aliased.
		/// </summary>
		protected Dictionary<string, object> GetFieldNames(ViewInfo view, string tableName, Dictionary<string, object> parms)
		{
			Dictionary<string, object> ret = new Dictionary<string, object>();

			// For each field in the view...
			view.Fields.Where(f => f.TableName == tableName).ForEach(vfi =>
				{
					// If the alias in the incoming params matches a view in the specified table...
					// (We have to deal with lowercase because the JSON response might be all in lowercase)
					// Then we add that field (un-aliased) and its value from the params to the returned collection.
					parms.SingleOrDefault(kvp => kvp.Key.ToLower() == vfi.Alias.ToLower()).IfTrue(kvp => !String.IsNullOrEmpty(kvp.Key), kvp2=> ret[vfi.FieldName] = kvp2.Value);
				});

			return ret;
		}
	}
}
