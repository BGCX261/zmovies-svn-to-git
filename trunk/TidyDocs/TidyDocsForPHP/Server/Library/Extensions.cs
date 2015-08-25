using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using MovieAgent.Server.Library;

namespace TidyDocsForPHP.Server.Library
{
    [Script]
    public static class Extensions
    {

        public static void ToConsole(this  string e)
        {
            Console.WriteLine(e);
        }

        public static string ToWebString(this string e)
        {
            var value = "";

            var u = new Uri(e);
            var c = new BasicWebCrawler(u.Host, 80);
            c.DataReceived +=
                document =>
                {
                    value = document;
                };

            c.Crawl(u.PathAndQuery);

            return value;
        }
    }
}
