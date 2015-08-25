using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using System.IO;

namespace TidyDocs
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.FirstOrDefault() == "appjet")
            {
                Console.WriteLine("creating install script...");

                Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, "web");

                // 100k is max
                // http://forum.appjet.com/search.php?search_id=1852971065
                using (var w = new StreamWriter(File.OpenWrite("AppJet.js")))
                {
                    w.BaseStream.SetLength(0);


                    w.WriteLine("/* appjet:version 0.1 */ ");

                    foreach (var k in SharedHelper.LocalModulesOf(typeof(Program).Assembly, ScriptType.JavaScript))
                    {
                        foreach (var x in File.ReadAllLines(k + ".js"))
                        {
                            var t = x.Trim();

                            var comment = t.IndexOf("//");

                            if (comment >= 0)
                                t = t.Substring(0, comment).Trim();

                            if (t.Length > 0)
                                w.Write(t);

                        }
                    }

                }

            }
        }
    }
}
