using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using ScriptCoreLib;
using ScriptCoreLib.Shared;
using MovieAgent.Client.Java;
using System.IO;
using System.Linq;

namespace MovieAgent
{
	static class Settings
	{

		public static void DefineEntryPoint(IEntryPoint e)
		{
			CreatePHPIndexPage(e, Server.Application.Filename, Server.Application.Application_Entrypoint);



		

		}

		#region PHP Section
		private static void CreatePHPIndexPage(IEntryPoint e, string file_name, Action entryfunction)
		{
			var a = new StringBuilder();

			a.AppendLine("<?");

			foreach (var u in SharedHelper.LocalModulesOf(Assembly.GetExecutingAssembly(), ScriptType.PHP))
			{
				a.AppendLine("require_once '" + u + ".php';");
			}

			a.AppendLine(entryfunction.Method.Name + "();");
			a.AppendLine("?>");


			e[file_name] = a.ToString();
		}
		#endregion


		#region Java Applet Section

		class AppletElementInfo
		{
			public string code { get; set; }
			public string codebase { get; set; }
			public string archive { get; set; }
			public int width { get; set; }
			public int height { get; set; }
			public string mayscript { get; set; }
		}

		class ParamInfo
		{
			public string name { get; set; }
			public object value { get; set; }
		}


		#region Applet HTML

		public static XElement ToElementWithAttributes(string tag, object attr, params object[] c)
		{
			return new XElement(tag,
				ToAttributes(attr).Concat(
					new object[] {
                        ""
                        }
				), c
			);
		}

		public static IEnumerable<XElement> ToParameters(object v)
		{
			foreach (PropertyInfo z in v.GetType().GetProperties())
			{
				yield return new XElement("param",
						ToAttributes(
							new ParamInfo
							{
								name = z.Name,
								value = z.GetValue(v, null)
							}
						)
					);
			}
		}

		public static IEnumerable<object> ToAttributes(object v)
		{
			foreach (PropertyInfo z in v.GetType().GetProperties())
			{
				yield return new XAttribute(z.Name, z.GetValue(v, null));
			}
		}

		#endregion

		public static void WriteSettings(StringWriter w, object v)
		{
			foreach (PropertyInfo z in v.GetType().GetProperties())
			{
				w.WriteLine("set {0}={1}", z.Name, z.GetValue(v, null));
			}
		}

		#endregion
	}

}
