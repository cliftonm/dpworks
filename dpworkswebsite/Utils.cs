using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;

namespace dpworkswebsite
{
	public static class Utils
	{
		const int SWP_NOSIZE = 0x0001;

		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		private static extern bool AllocConsole();

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetConsoleWindow();

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
			
			// And this is our fault:
			parms.Remove("__CSRFToken__");

			Dictionary<string, string> parms2 = new Dictionary<string, string>();

			// Also annoying -- spaces have been encoded with a "+", so we have to put them back to spaces.
			// TODO: Can this be avoided by using JSON for the data?
			parms.ForEach(kvp => parms2[kvp.Key] = kvp.Value.Replace("+", " "));

			return parms2;
		}

		// http://stackoverflow.com/questions/1548838/setting-position-of-a-console-window-opened-in-a-winforms-app/1548881#1548881
		public static void SetConsoleWindowPosition(int x, int y)
		{
			AllocConsole();
			IntPtr MyConsole = GetConsoleWindow();
			SetWindowPos(MyConsole, 0, x, y, 0, 0, SWP_NOSIZE);
		}
	}
}


