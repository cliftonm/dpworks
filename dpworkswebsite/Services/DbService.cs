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

	public class DbService
	{
		protected string connectionString;

		public DbService()
		{
			// Initialize the connection string from the app.config file.
			connectionString = ConfigurationManager.ConnectionStrings["ems"].ConnectionString;
		}

		/// <summary>
		/// Return the collection of records in the specified view.
		/// </summary>
		public DataTable Query(ViewInfo view)
		{
			DataTable dt = new DataTable();
			IDbConnection conn = OpenConnection();
			IDbCommand cmd = conn.CreateCommand();

			// Build the query:
			StringBuilder sb = new StringBuilder("select ");
			sb.Append(String.Join(", ", view.Fields.Select(f => f.FieldName)));
			sb.Append(" from ");
			sb.Append(view.TableName);

			// Get the data.
			cmd.CommandText = sb.ToString();
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
		public string QueryScalar(string sql, Dictionary<string, object> parms = null)
		{
			// The connection cannot be persisted as everything in these calls must be thread safe.
			IDbConnection conn = OpenConnection();
			IDbCommand cmd = conn.CreateCommand();
			cmd.CommandText = sql;
			parms.IfNotNull((p) => p.ForEach(kvp => cmd.Parameters.Add(new SqlParameter(kvp.Key, kvp.Value))));
			object obj = cmd.ExecuteScalar();

			string ret = (obj == DBNull.Value ? null : obj.ToString());

			return ret;
		}


		/// <summary>
		/// Return a hash of column name and value for the one and only row. 
		/// Expects that the row exists.
		/// </summary>
		public Dictionary<string, string> QuerySingleRow(string sql, Dictionary<string, object> parms = null)
		{
			Dictionary<string, string> ret = new Dictionary<string, string>();
			DataTable dt = Query(sql, parms);

			foreach (DataColumn dc in dt.Columns)
			{
				ret[dc.ColumnName] = dt.Rows[0][dc].ToString();
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

		public decimal InsertOrUpdate(string tableName, Dictionary<string, string> parms, string idField="ID")
		{
			decimal ret = -1;

			string pkField = idField.ToLower();

			// Get the primary key field and value.  We do a case-insensitve search but return the exact matching pkfield name.
			// Let the caller handle any exception.
			KeyValuePair<string, string> pkKvp = parms.Where(kvp => kvp.Key.ToLower() == pkField).SingleOrDefault();

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

		public decimal Insert(string tableName, Dictionary<string, string> parms)
		{
			// Get the connection and create the command:
			IDbConnection conn = OpenConnection();
			IDbCommand cmd = conn.CreateCommand();

			// Build the update statement:
			StringBuilder sql = new StringBuilder("insert into");
			sql.Append(tableName.Spaced());
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
		/// Update a record given the parameters and ID field.
		/// The ID field will be removed from the updated field set.
		/// The parms must include the ID field.
		/// </summary>
		public void Update(string tableName, Dictionary<string, string> parms, string idField = "ID")
		{
			KeyValuePair<string, string> pkKvp = GetPK(parms, idField);
			string pkField = pkKvp.Key;
			string pkValue = pkKvp.Value;

			// Copy the dictionary, so we can remove the PK value from this list.
			Dictionary<string, string> internalParms = new Dictionary<string, string>(parms);
			internalParms.Remove(pkField);

			// Get the connection and create the command:
			IDbConnection conn = OpenConnection();
			IDbCommand cmd = conn.CreateCommand();

			// Build the update statement:
			StringBuilder sql = new StringBuilder("update");
			sql.Append(tableName.Spaced());
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

		/// <summary>
		/// Deletes a record for the given PK value in the parms collection.
		/// We use the collection because it's convenient to pass in the entire set of record fields.
		/// </summary>
		public void Delete(string tableName, Dictionary<string, string> parms, string idField = "ID")
		{
			KeyValuePair<string, string> pkKvp = GetPK(parms, idField);
			string pkField = pkKvp.Key;
			string pkValue = pkKvp.Value;

			// Get the connection and create the command:
			IDbConnection conn = OpenConnection();
			IDbCommand cmd = conn.CreateCommand();

			cmd.CommandText = "delete from " + tableName + " where ID=@ID";
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

		protected KeyValuePair<string, string> GetPK(Dictionary<string, string> parms, string idField)
		{
			string pkField = idField.ToLower();

			// Get the primary key field and value.  We do a case-insensitve search but return the exact matching pkfield name.
			// Let the caller handle any exception.
			KeyValuePair<string, string> pkKvp = parms.Where(kvp => kvp.Key.ToLower() == pkField).Single();

			return pkKvp;
		}
	}
}
