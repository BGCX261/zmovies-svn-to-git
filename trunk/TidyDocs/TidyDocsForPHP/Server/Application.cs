using System;
using System.IO;
using System.Text;
using ScriptCoreLib;
using ScriptCoreLib.PHP;
using ScriptCoreLib.Shared;
using TidyDocsForPHP.Server.Library;

namespace TidyDocsForPHP.Server
{
    [Script]
    static class Application
    {
        public const string Filename = "index.php";

        //Alias /jsc/TidyDocsForPHP "C:\work\code.google\zmovies\TidyDocs\TidyDocsForPHP\bin\Debug\web"
        //<Directory "C:\work\code.google\zmovies\TidyDocs\TidyDocsForPHP\bin\Debug\web">
        //       Options Indexes FollowSymLinks ExecCGI
        //       AllowOverride All
        //       Order allow,deny
        //       Allow from all
        //</Directory>

        /// <summary>
        /// php script will invoke this method
        /// </summary>
        [Script(NoDecoration = true)]
        public static void Application_Entrypoint()
        {

            var QueryString = Native.QueryString;
            var Host = Native.SuperGlobals.Server[Native.SuperGlobals.ServerVariables.HTTP_HOST];

			Console.WriteLine("<!-- 20090611 -->");

            if (QueryString == "")
            {
                "<h1>tidy-<b>docs</b></h1>".ToConsole();


                "<style>b {color: blue;}</style><p>If you have a google document and you need it to be tidy, take the id shown in bold http://docs.google.com/Doc?id=<b>xyxyxyxyxyxyxy</b>&hl=en and go to /?<b>xyxyxyxyxyxyxy</b></p>".ToConsole();

                (@"<p><a  href='http://www.w3.org/QA/Tools/Donate'><img  src='http://www.w3.org/QA/Tools/I_heart_validator_lg'></a></p>").ToConsole();

                "<br /><a href='http://zproxy.wordpress.com'>created by zproxy</a>".ToConsole();

                return;
            }

			var leandoc = TransformDocument(QueryString);

            leandoc.ToConsole();
        }

		public static string TransformDocument(string QueryString)
		{
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


			var leandoc = cleandoc.Substring(0, cleandoc.IndexOf(footer_trigger_start))
				+ ("<p><a href='" + u.Replace("&", "&amp;") + "'>Google Docs</a></p>")
				+ ("<p><a href='" + tidydownload.Replace("&", "&amp;") + "'>Tidy</a></p>")
				+ (@"<center>
    <a href='http://validator.w3.org/check?uri=referer'><img
        src='http://www.w3.org/Icons/valid-xhtml10'
        height='31' width='88' /></a>
  </center>"
				)
				+ cleandoc.Substring(cleandoc.IndexOf(footer_trigger_end));

			leandoc = leandoc.Replace("\"File?id=", "\"http://docs.google.com/File?id=");

			leandoc = leandoc.Replace("<img", "<img alt='Image'");
			leandoc = leandoc.Replace("id=\"", "id=\"_");
			leandoc = leandoc.Replace("name=\"", "name=\"_");
			leandoc = leandoc.Replace("RANGE!", "RANGE_");

			// <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" > 

			if (!leandoc.Contains("<!DOCTYPE"))
				"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\" >".ToConsole();

			return leandoc;
		}
    }
}
