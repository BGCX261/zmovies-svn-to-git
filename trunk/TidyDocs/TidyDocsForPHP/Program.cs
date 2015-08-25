using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TidyDocsForPHP.Server;
using ScriptCoreLib;
using ScriptCoreLib.Shared;

namespace TidyDocsForPHP
{
    static class Program
    {
        public static void DefineEntryPoint(IEntryPoint e)
        {
            CreatePHPIndexPage(e, Application.Filename, Application.Application_Entrypoint);
        }


        #region PHP Section
        private static void CreatePHPIndexPage(IEntryPoint e, string file_name, Action entryfunction)
        {
            var w = new StringBuilder();

            w.AppendLine("<?");

            foreach (var kk in SharedHelper.LoadReferencedAssemblies(typeof(Application).Assembly, true))
            {
                var k = Path.GetFileName(kk.Location);
                Console.WriteLine("adding " + k);

                w.AppendLine("require_once '" + k + ".php';");
            }

            w.AppendLine(entryfunction.Method.Name + "();");

            w.AppendLine("?>");

            e[file_name] = w.ToString();
        }
        #endregion

		public static void Main(string[] args)
		{
			var doc = Application.TransformDocument("dfmc8fvr_303hk47c4fc");


		}
    }

}
