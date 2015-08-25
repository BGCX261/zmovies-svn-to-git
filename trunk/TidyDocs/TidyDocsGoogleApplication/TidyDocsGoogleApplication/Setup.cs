using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

using ScriptCoreLib;
using TidyDocsGoogleApplication.Server;
using ScriptCoreLibJava.Extensions;

namespace TidyDocsGoogleApplication
{
	class Setup
	{
		public const string SettingsFileName = "setup.settings.cmd";

		public static void DefineEntryPoint(IEntryPoint e)
		{
			e["java/WEB-INF/web.xml"] = typeof(TidyDocsServlet).Assembly.ToServletConfiguration();
		}

		public static void Main(string[] e)
		{
			var leandoc = File.ReadAllText("test.txt");

			leandoc = ReplaceString(leandoc, "name=\"", "name=\"_");

			leandoc = ReplaceString(leandoc, "id=\"", "id=\"_");
			leandoc = ReplaceString(leandoc, "RANGE!", "RANGE_");

		}

		public static string ReplaceString(string whom, string what, string with)
		{
			int j = -1;
			int i = whom.IndexOf(what);

			if (i == -1)
				return whom;

			var b = "";




			while (i > -1)
			{
				if (j < 0)
					b += whom.Substring(0, i ) + with;
				else
					b += whom.Substring(j + what.Length, i - j - what.Length) + with;

				j = i;
				i = whom.IndexOf(what, i + what.Length);
			}

			b += whom.Substring(j + what.Length);

			return b;
		}
	}
}
