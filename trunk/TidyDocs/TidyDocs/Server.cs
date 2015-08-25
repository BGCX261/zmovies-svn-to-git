using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptCoreLib;
using ScriptCoreLibAppJet;
using ScriptCoreLibAppJet.JavaScript.Library;
using TidyDocs.Library;
using ScriptCoreLibAppJet.JavaScript;

namespace TidyDocs
{
    [Script]
    public static class Server
    {
        [Script, Serializable]
        public sealed class DataItem
        {
            public string id;

            public string Key;
            public string Value;
        }

        [Script, Serializable]
        public sealed class SmartClient
        {
            public string id;

            public string url;
            public string data;
        }


        [Script(HasNoPrototype = true)]
        public class CoralHeaders
        {
            // Via	HTTP/1.0 131.246.191.42:8080 (CoralWebPrx/0.1.19 (See http://coralcdn.org/))
            public string Via;
            public string Host;
        }

        public static void Render()
        {
            // /* appjet:version 0.1 */ 

            Native.page.setMode("plain");


            var QueryString = Native.request.path.Substring(1);

            if (QueryString == "")
            {
                "<h1>tidy-docs</h1>".ToConsole();

                
                "<style>b {color: blue;}</style><p>If you have a google document and you need it to be tidy, take the id shown in bold http://docs.google.com/Doc?id=<b>xyxyxyxyxyxyxy</b>&hl=en and go to /<b>xyxyxyxyxyxyxy</b></p>".ToConsole();

                (@"<p><a  href='http://www.w3.org/QA/Tools/Donate'><img  src='http://www.w3.org/QA/Tools/I_heart_validator_lg'></a></p>").ToConsole();

                "<br /><a href='http://zproxy.wordpress.com'>created by zproxy</a>".ToConsole();

                return;
            }

            var ch = (CoralHeaders)Native.request.headers;

            //if (ch.Via == null || !ch.Via.Contains("http://coralcdn.org/"))
            //{
            //    Native.response.redirect("http://" + ch.Host + ".nyud.net/" + QueryString);
            //    return;
            //}


            var u = "http://docs.google.com/View?docID=" + QueryString + "&revision=_latest&hgd=1";



            var tidy = "http://infohound.net/tidy/tidy.pl?_function=tidy&_html=&_file=&alt-text=&clean=y&doctype=auto&drop-empty-paras=y&drop-proprietary-attributes=y&fix-backslash=y&fix-bad-comments=y&fix-uri=y&hide-comments=y&hide-endtags=y&join-styles=y&lower-literals=y&ncr=y&new-blocklevel-tags=&new-empty-tags=&new-inline-tags=&new-pre-tags=&output-xhtml=y&quote-ampersand=y&quote-nbsp=y&indent=auto&indent-spaces=2&tab-size=4&wrap=90&wrap-asp=y&wrap-jste=y&wrap-php=y&wrap-sections=y&ascii-chars=y&char-encoding=utf8&input-encoding=latin1&output-bom=auto&output-encoding=utf8&_output=warn";
            tidy += "&_url=" + u;


            var tidymanager = tidy.ToWebString();

            var trigger = "tidy.pl?_function=download&amp;file=";
            var trigger_i = tidymanager.IndexOf(trigger) + trigger.Length;
            var trigger_j = tidymanager.IndexOf("&amp;", trigger_i);
            var trigger_value = tidymanager.Substring(trigger_i, trigger_j - trigger_i);

            var tidydownload = "http://infohound.net/tidy/tidy.pl?_function=download&file=" + trigger_value;



            var cleandoc = tidydownload.ToWebString();

            //("<p> " + cleandoc.Length + " bytes</p>").ToConsole();
            //cleandoc = cleandoc.Replace("Arvo Sulakatko", "Ken Martin");

            //("<p> " + cleandoc.Length + " bytes</p>").ToConsole();



            var footer_trigger_start = "<div id=\"google-view-footer\">";
            var footer_trigger_end = "</body>";

            var validator = "http://validator.w3.org/check?uri=http://" + ch.Host + "/" + QueryString;

            var leandoc = cleandoc.Substring(0, cleandoc.IndexOf(footer_trigger_start))
                + ("<p><a href='" + u.Replace("&", "&amp;") + "'>Google Docs</a></p>")
                + ("<p><a href='" + tidydownload.Replace("&", "&amp;") + "'>Tidy</a></p>")
                + ("<p><a href='" + validator.Replace("&", "&amp;") + "'>Validator</a></p>")
                + (@"<center>
    <a href='http://validator.w3.org/check?uri=referer'><img
        src='http://www.w3.org/Icons/valid-xhtml10'
        alt='Valid XHTML 1.0 Transitional' height='31' width='88' /></a>
  </center>"
                )
                + cleandoc.Substring(cleandoc.IndexOf(footer_trigger_end));

            leandoc = leandoc.Replace("id=\"", "id=\"_");
            leandoc = leandoc.Replace("name=\"", "name=\"_");
            leandoc.ToConsole();
        }

        static Server()
        {
            Native.import("storage");
            Render();
        }
    }
}
