using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dpworkswebsite.Models
{
	public class ViewFieldInfo
	{
		public string FieldName { get; set; }
		public string Caption { get; set; }
		public string Width { get; set; }		// TODO: Kludgy to put here?
		public bool Visible { get; set; }
		public Type DataType { get; set; }
		public bool IsPK { get; set; }
		public bool IsNullable { get; set; }

		public ViewFieldInfo()
		{
			Visible = true;
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
