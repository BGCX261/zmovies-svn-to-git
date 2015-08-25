using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using ScriptCoreLibAppJet.JavaScript;

namespace TidyDocs.Library
{
    [Script]
    public static class Extensions
    {
        public static string ReplaceString(this string whom, string what, string with)
        {
            int j = -1;
            int i = whom.IndexOf(what);

            if (i == -1)
                return whom;

            var b = "";




            while (i > -1)
            {
                b += whom.Substring(j + what.Length, i - j - what.Length) + with;

                j = i;
                i = whom.IndexOf(what, i + what.Length);
            }

            b += whom.Substring(j + what.Length);

            return b;
        }

        public static string ToWebString(this string src)
        {
            var r = Native.wget(src, null, new WebRequestOptions());


            return r.data;
        }

        public static string ToImage(this string src)
        {
            return "<img src='" + src + "' />";
        }
    }
}
