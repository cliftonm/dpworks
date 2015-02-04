using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpworkswebsite.Models
{
	/// <summary>
	/// Used for handling foreign key lookups.
	/// </summary>
	public class LookupInfo
	{
		public string TableName { get; set; }
		public string IdFieldName { get; set; }
		public string ValueFieldName { get; set; }
	}

	public class ViewFieldInfo
	{
		public string FieldName { get; set; }
		public string Caption { get; set; }
		public string Width { get; set; }		// TODO: Kludgy to put here?
		public bool Visible { get; set; }
		public Type DataType { get; set; }
		public bool IsPK { get; set; }
		public bool IsNullable { get; set; }
		public LookupInfo LookupInfo { get; set; }
		public string SqlFormat { get; set; }

		/// <summary>
		/// Default value is required on insert of an empty record or missing data.
		/// This is tied to the jqxDataTable behavior -- when the user presses "add", we get an "addrecord" AJAX callback!
		/// Unfortunately, this means that fields I'd like to define as not-nullable end up getting empty string values.  Sigh.
		/// </summary>
		public object DefaultValue { get; set; }

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
			}

			return ret;
		}
	}

	public class ViewInfo
	{
		public string TableName { get; set; }
		public List<ViewFieldInfo> Fields { get; protected set; }

		public ViewInfo()
		{
			Fields = new List<ViewFieldInfo>();
		}
	}
}
