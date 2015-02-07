using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.WebServer;

namespace dpworkswebsite.Models
{
	/// <summary>
	/// Used for handling foreign key lookups.
	/// </summary>
	public class LookupInfo
	{
		public string ViewName { get; set; }
		public string IdFieldName { get; set; }
		public string ValueFieldName { get; set; }
		
		// The URL for populating the lookup information.
		public string Url { get; set; }
	}

	public class ViewFieldInfo
	{
		protected string alias;

		public string TableName { get; set; }
		public string FieldName { get; set; }
		public string Caption { get; set; }
		public string Width { get; set; }		// TODO: Kludgy to put here?
		public bool Visible { get; set; }
		public Type DataType { get; set; }
		public bool IsPK { get; set; }
		public bool IsFK { get { return LookupInfo != null; } }
		public bool IsNullable { get; set; }

		// Foreign key specification.
		public LookupInfo LookupInfo { get; set; }

		/// <summary>
		/// Optional field that requires a SQL-server operation.
		/// </summary>
		public string SqlFormat { get; set; }

		/// <summary>
		/// Optional alias, required to when fields in a table join have the same name and must be resolved.
		/// The return value is the alias (if defined) or the field name (if alias is not defined.)
		/// </summary>
		public string Alias 
		{
			get {return !String.IsNullOrEmpty(alias) ? alias : FieldName;}
			set { alias = value; }
		}

		/// <summary>
		/// Default value is required on insert of an empty record or missing data.
		/// This is tied to the jqxDataTable behavior -- when the user presses "add", we get an "addrecord" AJAX callback!
		/// Unfortunately, this means that fields I'd like to define as not-nullable end up getting empty string values.  Sigh.
		/// </summary>
		public object DefaultValue { get; set; }

		/// <summary>
		/// If the value must be computed in realtime, use this property.
		/// </summary>
		public Func<Session, object> ComputedValue { get; set; }

		public ViewFieldInfo()
		{
			Visible = true;
			DefaultValue = null;
		}

		public string GetJavascriptType()
		{
			string ret = "string";			// the default.
			string type = DataType.Name.ToLower();

			switch (type)
			{
				case "string":
					ret = "string";
					break;

				case "int32":
				case "int64":
				case "decimal":
					ret = "integer";
					break;

				case "bool":
					ret = "boolean";
					break;

				case "datetime":
					ret = "string";
					break;
			}

			return ret;
		}
	}

	public class ViewInfo
	{
		public string Name { get; set; }

		/// <summary>
		/// Use this property for specifying single or joined tables, otherwise leave it null.
		/// The writeable table should the first table.  Eventually, the code will support writing across all tables in the view.
		/// The programmer must configure aliases for fields with the same name.
		/// </summary>
		public List<string> Tables { get; set; }

		// TODO: Should this really be registered?
		public static Dictionary<string, ViewInfo> RegisteredViews = new Dictionary<string, ViewInfo>();

		/// <summary>
		/// The collection of fields in this view.
		/// </summary>
		public List<ViewFieldInfo> Fields { get; protected set; }

		public ViewInfo()
		{
			Tables = new List<string>();
			Fields = new List<ViewFieldInfo>();
		}

		public void Register()
		{
			RegisteredViews[Name] = this;
		}
	}
}
